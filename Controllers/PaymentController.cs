using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Pegasus_backend.ActionFilter;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BasicController
    {
        private IMapper _mapper;
        public PaymentController(ablemusicContext ablemusicContext, ILogger<PaymentController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }


        [HttpGet("[action]/{staffId}/{beginDate}/{endDate}")]
        public async Task<IActionResult> PaymentByDate(short staffId,DateTime beginDate, DateTime endDate)
        {
            Result<Object> result = new Result<object>();
            try
             {
                var orgs = await _ablemusicContext.StaffOrg.Where(o=>o.StaffId==staffId).Select(o=>o.OrgId).ToListAsync();
                var payments = await _ablemusicContext.Payment
                    .Where(d => d.CreatedAt >beginDate && d.CreatedAt <endDate
                        && orgs.Contains(d.Staff.StaffOrg.FirstOrDefault().OrgId))
                     .Include(p => p.Invoice)
                     .Include(p => p.Learner)                     
                     .Include(p => p.SoldTransaction ).ThenInclude(p => p.Product)
                     .Include(t => t.Staff ).ToListAsync();
 //&& orgs.Contains(d.Staff.StaffOrg.FirstOrDefault().OrgId)
                result.Data = _mapper.Map<List<GetPaymentModel>>(payments);

              }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        [CheckModelFilter]
        [HttpPost]
        [Route("payInvoice")]
        
        public async Task<IActionResult> SavePaymentDetails([FromBody] InvoicePay details)
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var invoiceItem =
                        await _ablemusicContext.Invoice.FirstOrDefaultAsync(s => s.InvoiceId == details.InvoiceId);
                    if (invoiceItem == null)
                    {
                        throw new Exception("Invoice does not found.");
                    }

                    //the owing fee for invoice cannot be negative.
                    if (invoiceItem.OwingFee - details.Amount < 0)
                    {
                        throw new Exception("You only need to pay " + invoiceItem.OwingFee + " dollar. No more than it");
                    }

                    invoiceItem.PaidFee = invoiceItem.PaidFee + details.Amount;
                    invoiceItem.OwingFee = invoiceItem.OwingFee - details.Amount;
                    if (invoiceItem.OwingFee > 0)
                    {
                        invoiceItem.IsPaid = 0;
                    }

                    if (invoiceItem.OwingFee == 0)
                    {
                        invoiceItem.IsPaid = 1;
                    }
                    _ablemusicContext.Update(invoiceItem);
                    await _ablemusicContext.SaveChangesAsync();

                    //save the Invoice payment to Payment table
                    var paymentItem = new Payment();
                    _mapper.Map(details, paymentItem);
                    paymentItem.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    paymentItem.PaymentType = 1;
                    _ablemusicContext.Add(paymentItem);
                    await _ablemusicContext.SaveChangesAsync();

                    var fundItem =
                        await _ablemusicContext.Fund.FirstOrDefaultAsync(s => s.LearnerId == details.LearnerId);
                    fundItem.Balance = fundItem.Balance + details.Amount;
                    fundItem.UpdatedAt = toNZTimezone(DateTime.UtcNow);
                    _ablemusicContext.Update(fundItem);
                    await _ablemusicContext.SaveChangesAsync();

                    if (invoiceItem.IsPaid == 1)
                    {
                        if (invoiceItem.CourseInstanceId!=null) { //if this is a one on one session 
                            var lessonRemain = new LessonRemain
                            {
                                Quantity = invoiceItem.LessonQuantity,
                                TermId = invoiceItem.TermId,
                                ExpiryDate = invoiceItem.EndDate.Value.AddMonths(3),
                                CourseInstanceId = invoiceItem.CourseInstanceId,
                                LearnerId = invoiceItem.LearnerId
                            };
                            _ablemusicContext.Add(lessonRemain);
                        }
                        if  (invoiceItem.CourseInstanceId != null)
                            await SaveLesson(details.InvoiceId,0,1);
                        await _ablemusicContext.SaveChangesAsync();

                    }
                    dbContextTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            result.Data = "Success!";
            return Ok(result);
        }


        [HttpPost]
        [Route("[action]")]
        [CheckModelFilter]
        public async Task<IActionResult> SaveProdPayment([FromForm(Name = "paymentTranList")] string paymentTranList)
        {
            Result<string> result = new Result<string>();
            try
            {
                var paymentTranListJson = JsonConvert.DeserializeObject<PaymentTranModel>(paymentTranList);
                Payment payment = new Payment();
                _mapper.Map(paymentTranListJson, payment);
                payment.CreatedAt = toNZTimezone(DateTime.UtcNow);
                payment.PaymentType = 2;
                int i = 0;
                decimal? amount = 0;
                foreach (var detail in payment.SoldTransaction)
                {
                    var stock = await _ablemusicContext.Stock.FirstOrDefaultAsync(x => x.OrgId == paymentTranListJson.OrgId && x.ProductId == detail.ProductId);
                    var name = await _ablemusicContext.Product.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);
                    //return Ok(stock);
                    if (stock == null)
                    {
                        throw new Exception(name.ProductName + " is out of stock");
                    }
                    detail.BeforeQuantity = stock.Quantity;
                    detail.AflterQuantity = detail.BeforeQuantity - detail.SoldQuantity;
                    detail.LearnerId = paymentTranListJson.LearnerId;
                    detail.PaymentId = payment.PaymentId;

                    if (detail.AflterQuantity < 0)
                    {
                        throw new Exception(name.ProductName + " has not enough stock, only " + stock.Quantity + " left");
                    }
                    detail.StockId = stock.StockId;
                    detail.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    detail.DiscountedAmount = name.SellPrice * detail.SoldQuantity;
                    if (detail.DiscountAmount != 0)
                    {
                        detail.DiscountedAmount -= detail.DiscountAmount;
                    }
                    else if (detail.DiscountRate != 1)
                    {
                        if (detail.DiscountRate > 1)
                        {
                            throw new Exception("Discount Rate must less than 1");
                        }
                        detail.DiscountedAmount *= detail.DiscountRate;
                    }

                    stock.Quantity -= detail.SoldQuantity;
                    _ablemusicContext.Stock.Update(stock);
                    _mapper.Map(detail, payment.SoldTransaction.ToArray()[i]);
                    i++;
                    amount += detail.DiscountedAmount;

                }
                await _ablemusicContext.Payment.AddAsync(payment);
                await _ablemusicContext.SaveChangesAsync();
                if (amount != payment.Amount)
                {
                    throw new Exception("Amount Error!");
                }

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }


        //auto-generate lessons sort by invoice when the invoice is paid
        private async Task<int> SaveLesson(int invoice_id, int isWaitingConfirm, int isOne2one)
        {
            var result = new Result<object>();
            var invoice = new WaitingOrInvoice();
            if (isWaitingConfirm == 1)
            {
                var tem = await _ablemusicContext.InvoiceWaitingConfirm.FirstOrDefaultAsync(x => x.WaitingId == invoice_id);
                _mapper.Map(tem,invoice);
            }
            else
            {
                var tem = await _ablemusicContext.Invoice.FirstOrDefaultAsync(x => x.InvoiceId == invoice_id);
                _mapper.Map(tem,invoice);
            }

            var course = new OneOrGroupCourse();
            var schedules = await _ablemusicContext.CourseSchedule.Where(x => x.CourseInstanceId == invoice.CourseInstanceId).OrderBy(x => x.DayOfWeek).ToArrayAsync();
            var amendments = await _ablemusicContext.Amendment.Where(x => x.CourseInstanceId == invoice.CourseInstanceId && x.BeginDate <= invoice.EndDate).OrderBy(x => x.CreatedAt).ToArrayAsync();

            if (isOne2one == 1)
            {
                var cour = await _ablemusicContext.One2oneCourseInstance.FirstOrDefaultAsync(x => x.CourseInstanceId == invoice.CourseInstanceId);
                _mapper.Map(cour, course);
 
            }
            else
            {
                var cour = await _ablemusicContext.GroupCourseInstance.FirstOrDefaultAsync(x => x.GroupCourseInstanceId == invoice.GroupCourseInstanceId);
                _mapper.Map(cour, course);
                schedules = await _ablemusicContext.CourseSchedule.Where(x => x.GroupCourseInstanceId == invoice.GroupCourseInstanceId).OrderBy(x => x.DayOfWeek).ToArrayAsync();
                amendments = null;
            }

            
            var holiday = await _ablemusicContext.Holiday.Select(x => x.HolidayDate).ToArrayAsync();

            DateTime begindate_invoice = (DateTime)invoice.BeginDate;
            //get the day of week of the begindate in invoice
            int DayOfWeek_invoice = day_trans(begindate_invoice.DayOfWeek.ToString());

            //save the begindate of each lesson[each schedule]
            DateTime[] lesson_begindate = new DateTime[schedules.Length];
            //count the week of course
            int num = 0;
            int lesson_quantity = 0;
            int course_week = 0;
            if (invoice.LessonQuantity == null || invoice.LessonQuantity == 0)
            {
                TimeSpan time = (TimeSpan)(invoice.EndDate - invoice.BeginDate);
                course_week = (time.Days / 7) + 1;
                invoice.LessonQuantity = course_week * schedules.Length;
            }
            int outofday = 0;
            for (int i = 0; i < invoice.LessonQuantity;)
            {
                int lesson_flag = 0;

                foreach (var schedule in schedules)
                {
                    int flag = 0;
                    int count = 0; //count the day between invoice begindate and course date

                    //calculated the begindate of the course
                    if (DayOfWeek_invoice > (int)schedule.DayOfWeek)
                    {
                        count = (int)(7 - DayOfWeek_invoice + schedule.DayOfWeek);
                        lesson_begindate[lesson_flag] = begindate_invoice.AddDays(count + 7 * num);
                    }
                    else if (DayOfWeek_invoice <= (int)schedule.DayOfWeek)
                    {
                        count = (int)(schedule.DayOfWeek - DayOfWeek_invoice);
                        lesson_begindate[lesson_flag] = begindate_invoice.AddDays(count + 7 * num);
                    }

                    lesson_begindate[lesson_flag] = Convert.ToDateTime(lesson_begindate[lesson_flag].ToString("yyyy-MM-dd"));

                    //begin to generate the lesson
                    try
                    {
                        Lesson lesson = new Lesson();
                        lesson.CourseInstanceId = invoice.CourseInstanceId;
                        lesson.CreatedAt = toNZTimezone(DateTime.UtcNow);
                        lesson.RoomId = course.RoomId;
                        lesson.OrgId = (short)course.OrgId;
                        lesson.TeacherId = course.TeacherId;
                        lesson.LearnerId = (int)invoice.LearnerId;
                        lesson.InvoiceId = invoice.InvoiceId;
                        lesson.IsConfirm = 0;
                        lesson.IsCanceled = 0; 
                        lesson.IsTrial = 0;                                                                       

                        string begintime = "";
                        string endtime = "";

                        //if the lesson has been motified
                        if (amendments != null)
                        {
                            int flag_DOW = 0;
                            int amend_conflict = 0;

                            foreach (var amendment in amendments)
                            {

                                if (lesson_begindate[lesson_flag] >= amendment.BeginDate && (lesson_begindate[lesson_flag] <= amendment.EndDate || amendment.EndDate==null))
                                {
                                    if (amendment.AmendType == 1)
                                    {
                                        if (amendment.CourseScheduleId == null || (amendment.CourseScheduleId != null && amendment.CourseScheduleId == schedule.CourseScheduleId))
                                        {
                                            if (lesson_begindate[lesson_flag] <= amendment.EndDate)
                                            {
                                                flag = 4;
                                            }
                                        }
                                    }

                                    else if (amendment.AmendType == 2 && schedule.CourseScheduleId == amendment.CourseScheduleId && amend_conflict==0)
                                    {
                                        count = 0;
                                        count =(int)amendment.DayOfWeek- (int)schedule.DayOfWeek;
                                        lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(count);

                                        begintime = amendment.BeginTime.ToString();
                                        endtime = amendment.EndTime.ToString();
                                        flag = 1;
                                        flag_DOW = (int)amendment.DayOfWeek;
                                        lesson.RoomId = amendment.RoomId;
                                        lesson.OrgId = (short)amendment.OrgId;
                                        lesson.TeacherId = amendment.TeacherId;
                                        amend_conflict++;

                                    }
                                    else if(amendment.AmendType == 2 && schedule.CourseScheduleId == amendment.CourseScheduleId && amend_conflict!=0)
                                    {
                                        count = (int)amendment.DayOfWeek - flag_DOW;
                                        lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(count);
                                        begintime = amendment.BeginTime.ToString();
                                        endtime = amendment.EndTime.ToString();
                                        lesson.RoomId = amendment.RoomId;
                                        lesson.OrgId = (short)amendment.OrgId;
                                        lesson.TeacherId = amendment.TeacherId;

                                    }
                                }
                            }
                        }

                        //if the lesson date is holiday, then skip this date
                        if (holiday != null)
                        {
                            foreach (var ele in holiday)
                            {
                                Boolean is_Equal = string.Equals(lesson_begindate[lesson_flag].ToString("yyyy-MM-dd"), ele.ToString("yyyy-MM-dd"));
                                if (is_Equal == true)
                                {
                                    flag = 4;
                                    break;
                                }
                            }
                        }
                        if (flag == 4) 
                        { 
                            lesson_flag++; 
                            continue; 
                        }


                        if (outofday == schedules.Length)
                        {
                            i = (int)invoice.LessonQuantity;
                            break;
                        }

                        if (lesson_begindate[lesson_flag] > invoice.EndDate)
                        {
                            outofday++;
                            lesson_flag++;
                            continue;
                        }


                        string lesson_begindate_result = lesson_begindate[lesson_flag].ToString("yyyy-MM-dd");
                        //Concat the datetime, date from invoice and time from schedule
                        if (flag == 0)
                        {
                            begintime = schedule.BeginTime.ToString();
                            endtime = schedule.EndTime.ToString();

                        }

                        string beginDate = string.Concat(lesson_begindate_result, " ", begintime);
                        string endDate = string.Concat(lesson_begindate_result, " ", endtime);
                        DateTime BeginTime = Convert.ToDateTime(beginDate);
                        DateTime EndTime = Convert.ToDateTime(endDate);
                        lesson.BeginTime = BeginTime;
                        lesson.EndTime = EndTime;
                        await _ablemusicContext.Lesson.AddAsync(lesson);
                        await _ablemusicContext.SaveChangesAsync();
                        lesson_quantity++;
                    }
                    catch (Exception e)
                    {
                        result.ErrorMessage = e.Message;
                        result.IsSuccess = false;
                        result.IsFound = false;
                    }
                    i++;

                    if (i >= invoice.LessonQuantity) break;
                    lesson_flag++;
                }
                num++;
            }
            //await _ablemusicContext.SaveChangesAsync();
            return lesson_quantity;
        }

        private int day_trans(string day)
        {
            int day_num = 0;
            switch (day)
            {
                case "Monday":
                    day_num = 1;
                    break;
                case "Tuesday":
                    day_num = 2;
                    break;
                case "Wednesday":
                    day_num = 3;
                    break;
                case "Thursday":
                    day_num = 4;
                    break;
                case "Friday":
                    day_num = 5;
                    break;
                case "Saturday":
                    day_num = 6;
                    break;
                case "Sunday":
                    day_num = 7;
                    break;
            }

            return day_num;
        }

        [HttpPost]
        [Route("[action]/{term_id}/{instance_id?}")]
        public async Task<IActionResult> Generateone2oneInvoice(int term_id, int instance_id=0)
        {
            var result = new Result<object>();
            var course_instances = await _ablemusicContext.One2oneCourseInstance
                .Include(x => x.Course)
                .Include(x => x.Learner).Where(x => x.Learner.IsActive == 1)
                .Select(x => new
                {
                    x.LearnerId,
                    x.CourseId,
                    x.CourseInstanceId,
                    x.BeginDate,
                    x.EndDate,
                    x.InvoiceDate,
                    Course = new
                    {
                        x.Course.CourseName,
                        x.Course.Price
                    },
                    Learner = new
                    {
                        x.Learner.FirstName,
                        x.Learner.PaymentPeriod
                    }

                })
                .ToListAsync();
            if (instance_id != 0)
            {
                course_instances = course_instances.Where(x => x.CourseInstanceId == instance_id).ToList();
            }

            var term = await _ablemusicContext.Term.FirstOrDefaultAsync(x => x.TermId == term_id);

            var all_terms = await _ablemusicContext.Term.Select(x => new { x.TermId, x.BeginDate, x.EndDate }).ToListAsync();
            int i = 0;
            foreach (var course_instance in course_instances)
            {
                if (course_instance.InvoiceDate >= Convert.ToDateTime(term.EndDate)) continue;
                InvoiceWaitingConfirm invoice = new InvoiceWaitingConfirm();

                invoice.LearnerId = course_instance.LearnerId;
                invoice.LearnerName = course_instance.Learner.FirstName;
                invoice.CourseInstanceId = course_instance.CourseInstanceId;
                invoice.CourseName = course_instance.Course.CourseName;
                invoice.TermId = (short)term_id;
                invoice.IsPaid = 0;
                invoice.PaidFee = 0;
                invoice.CreatedAt = toNZTimezone(DateTime.UtcNow);
                invoice.IsConfirmed = 0;
                invoice.IsActivate = 1;
                invoice.IsEmailSent = 0;

                var courseIns = await _ablemusicContext.One2oneCourseInstance.FirstOrDefaultAsync(x => x.CourseInstanceId == invoice.CourseInstanceId);
                int lesson_quantity = 0;

                if (course_instance.Learner.PaymentPeriod == 1 && (course_instance.InvoiceDate == null || course_instance.InvoiceDate < term.EndDate))
                {
                    if (course_instance.BeginDate >= term.BeginDate)
                    {
                        invoice.BeginDate = course_instance.BeginDate;
                    }
                    else
                    {
                        invoice.BeginDate = term.BeginDate;
                    }

                    invoice.EndDate = term.EndDate;

                    await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoice);
                    await _ablemusicContext.SaveChangesAsync();
                    using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                    {
                        lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 1);
                        dbContextTransaction.Rollback();

                    }
                    courseIns.InvoiceDate = invoice.EndDate;
                }
                else if (course_instance.Learner.PaymentPeriod == 2)
                {
                    if (course_instance.InvoiceDate == null)
                    {
                        if (course_instance.BeginDate >= term.BeginDate)
                        {
                            invoice.BeginDate = course_instance.BeginDate;
                        }
                        else
                        {
                            invoice.BeginDate = term.BeginDate;
                        }
                        int DOW = day_trans(Convert.ToDateTime(invoice.BeginDate).DayOfWeek.ToString());

                        invoice.BeginDate = Convert.ToDateTime(invoice.BeginDate).AddDays(8 - DOW);
                        invoice.EndDate = Convert.ToDateTime(invoice.BeginDate).AddDays(6);

                        courseIns.InvoiceDate = invoice.EndDate;

                    }
                    else if (course_instance.EndDate == null || (course_instance.EndDate != null && course_instance.EndDate > course_instance.InvoiceDate))
                    {

                        invoice.BeginDate = Convert.ToDateTime(courseIns.InvoiceDate).AddDays(1);
                        invoice.EndDate = Convert.ToDateTime(invoice.BeginDate).AddDays(6);
                        courseIns.InvoiceDate = invoice.EndDate;
                    }
                    else continue;
                    foreach (var all_term in all_terms)
                    {
                        if (invoice.EndDate >= all_term.BeginDate && invoice.EndDate <= all_term.EndDate) invoice.TermId = all_term.TermId;
                    }
                    await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoice);
                    await _ablemusicContext.SaveChangesAsync();
                    using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                    {
                        lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 1);
                        dbContextTransaction.Rollback();

                    }
                }
                if (invoice.BeginDate != null) invoice.DueDate = Convert.ToDateTime(invoice.BeginDate).AddDays(-1);
                invoice.LessonFee = course_instance.Course.Price * lesson_quantity;
                invoice.LessonFee = course_instance.Course.Price * lesson_quantity;
                invoice.OwingFee = invoice.LessonFee;
                invoice.TotalFee = invoice.LessonFee;
                invoice.LessonQuantity = lesson_quantity;
                if (invoice.LessonFee <= 0) continue;
                _ablemusicContext.InvoiceWaitingConfirm.Update(invoice);
                invoice.InvoiceNum = invoice.WaitingId.ToString();
                _ablemusicContext.Update(courseIns);

                await _ablemusicContext.SaveChangesAsync();

                i++;
                //if (i == 4) break;

            }
            result.Data = i;
            return Ok(result);
        }

        [HttpPost]
        [Route("[action]/{term_id}/{instance_id?}")]
        public async Task<IActionResult> GenerateGroupInvoice(int term_id, int instance_id=0)
        {
            var result = new Result<object>();
            var group_course_instances = await _ablemusicContext.GroupCourseInstance
                .Include(x => x.Course)
                .Include(x => x.LearnerGroupCourse)
                .Select(x => new
                {
                    x.CourseId,
                    x.GroupCourseInstanceId,
                    x.BeginDate,
                    x.EndDate,
                    x.IsStarted,
                    CourseName=x.Course.CourseName,
                    Price=x.Course.Price,
                    Learners=x.LearnerGroupCourse.Select(s=>new {s.Learner.FirstName,s.LearnerId,s.CreatedAt,s.BeginDate,s.EndDate,s.InvoiceDate,s.LearnerGroupCourseId,s.IsActivate}).Where(s=>s.IsActivate==1).ToArray()

                })
                .ToListAsync();
            if(instance_id != 0)
            {
                group_course_instances=group_course_instances.Where(x => x.GroupCourseInstanceId == instance_id).ToList();
            }

            var term = await _ablemusicContext.Term.FirstOrDefaultAsync(x => x.TermId == term_id);
            int i = 0;
            //int j = 0;
            foreach (var group_course_instance in group_course_instances)
            {
                foreach (var learner in group_course_instance.Learners)
                {
                    if (learner.InvoiceDate >= Convert.ToDateTime(term.EndDate)) continue;
                    DateTime begin_date;
                    InvoiceWaitingConfirm invoice = new InvoiceWaitingConfirm();

                    invoice.LearnerId = learner.LearnerId;
                    invoice.LearnerName = learner.FirstName;
                    invoice.GroupCourseInstanceId = group_course_instance.GroupCourseInstanceId;
                    invoice.CourseName = group_course_instance.CourseName;
                    invoice.TermId = (short)term.TermId;
                    invoice.IsPaid = 0;
                    invoice.PaidFee = 0;
                    invoice.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    invoice.IsConfirmed = 0;
                    invoice.IsActivate = 1;
                    invoice.IsEmailSent = 0;

                    var courseIns = await _ablemusicContext.LearnerGroupCourse.FirstOrDefaultAsync(x => x.LearnerGroupCourseId == learner.LearnerGroupCourseId);
                    int lesson_quantity = 0;

                    if (learner.InvoiceDate == null  || (learner.InvoiceDate < term.EndDate && learner.BeginDate<=term.EndDate))
                    {
                        if (learner.BeginDate >= term.BeginDate)
                        {
                            begin_date = (DateTime)learner.BeginDate;
                        }
                        else
                        {
                            begin_date = (DateTime)term.BeginDate;
                        }
                        invoice.BeginDate = begin_date;
                        invoice.EndDate = term.EndDate;

                        await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoice);
                        await _ablemusicContext.SaveChangesAsync();
                        using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                        {
                            lesson_quantity = await SaveLesson(invoice.WaitingId, 1,0);
                            dbContextTransaction.Rollback();

                        }
                        courseIns.InvoiceDate=invoice.EndDate;
                    }
                        

                    if (invoice.BeginDate != null) invoice.DueDate = Convert.ToDateTime(invoice.BeginDate).AddDays(-1);
                    invoice.LessonFee = group_course_instance.Price * lesson_quantity;

                    invoice.OwingFee = invoice.LessonFee;
                    invoice.TotalFee = invoice.LessonFee;
                    invoice.LessonQuantity = lesson_quantity;
                    if (invoice.LessonFee <= 0) continue;
                    _ablemusicContext.InvoiceWaitingConfirm.Update(invoice);
                    invoice.InvoiceNum = invoice.WaitingId.ToString();
                    _ablemusicContext.Update(courseIns);

                    await _ablemusicContext.SaveChangesAsync();

                    i++;
                    //j++;
                    //if (i == 5) break;
                }
                //if (j > 1) break;
            }
            result.Data = i;

            return Ok(result);

        }

        [HttpGet]
        public async Task<ActionResult> GetTerms()
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.Term
                   
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpPut("{paymentId}")]
        public async Task<IActionResult> Put(int paymentId,[FromBody] string comment)
        {
            var result = new Result<Payment>();
            var payment = new Payment();
            try
            {
                payment = await _ablemusicContext.Payment.Where(p => p.PaymentId == paymentId).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(payment == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "payment not found";
                return BadRequest(result);
            }
            payment.IsConfirmed = 1;
            payment.Comment = comment;
            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = payment;
            return Ok(result);
        }
    }
}