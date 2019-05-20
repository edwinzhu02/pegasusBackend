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

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController: ControllerBase
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private IMapper _mapper;
        public PaymentController(pegasusContext.ablemusicContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        
        //POST: http://localhost:5000/api/payment/payInvoice
        
        [CheckModelFilter]
        [HttpPost]
        [Route("payInvoice")]
        public async Task<IActionResult> SavePaymentDetails([FromBody] InvoicePay details)
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var invoiceItem =
                        await _pegasusContext.Invoice.FirstOrDefaultAsync(s => s.InvoiceId == details.InvoiceId);
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
                    _pegasusContext.Update(invoiceItem);
                    await _pegasusContext.SaveChangesAsync();

                    //save the Invoice payment to Payment table
                    var paymentItem = new Payment();
                    _mapper.Map(details, paymentItem);
                    paymentItem.CreatedAt = DateTime.Now;
                    paymentItem.PaymentType = 1;
                    _pegasusContext.Add(paymentItem);
                    await _pegasusContext.SaveChangesAsync();

                    var fundItem =
                        await _pegasusContext.Fund.FirstOrDefaultAsync(s => s.LearnerId == details.LearnerId);
                    fundItem.Balance = fundItem.Balance + details.Amount;
                    fundItem.UpdatedAt = DateTime.Now;
                    _pegasusContext.Update(fundItem);
                    await _pegasusContext.SaveChangesAsync();

                    if (invoiceItem.IsPaid == 1)
                    {
                        var lessonRemain = new LessonRemain
                        {
                            Quantity = invoiceItem.LessonQuantity,
                            TermId = invoiceItem.TermId,
                            ExpiryDate = invoiceItem.EndDate.Value.AddMonths(3),
                            CourseInstanceId = invoiceItem.CourseInstanceId,
                            LearnerId = invoiceItem.LearnerId
                        };
                        _pegasusContext.Add(lessonRemain);
                        await _pegasusContext.SaveChangesAsync();
                        await SaveLesson(details.InvoiceId);
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
        public async Task<IActionResult> SaveProdPayment([FromForm (Name = "paymentTranList")] string paymentTranList)
        {
            Result<string> result = new Result<string>();
            try{
                var paymentTranListJson = JsonConvert.DeserializeObject<PaymentTranModel>(paymentTranList);
                Payment payment = new Payment();
                _mapper.Map(paymentTranListJson, payment);
                payment.CreatedAt = DateTime.Now;
                payment.PaymentType = 2;
                int i = 0;
                decimal? amount = 0;
                foreach (var detail in payment.SoldTransaction)
                {
                    var stock =await _pegasusContext.Stock.FirstOrDefaultAsync(x => x.OrgId == paymentTranListJson.OrgId && x.ProductId == detail.ProductId);
                    var name = await _pegasusContext.Product.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);
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
                    detail.CreatedAt = DateTime.Now;
                    detail.DiscountedAmount = name.SellPrice * detail.SoldQuantity;
                    if (detail.DiscountAmount != 0)
                    {
                        detail.DiscountedAmount -= detail.DiscountAmount;
                    }
                    else if (detail.DiscountRate != 1)
                    {
                        if(detail.DiscountRate > 1)
                        {
                            throw new Exception("Discount Rate must less than 1");
                        }
                        detail.DiscountedAmount *= detail.DiscountRate;
                    }
                          
                    stock.Quantity -= detail.SoldQuantity;
                    _pegasusContext.Stock.Update(stock);
                    _mapper.Map(detail, payment.SoldTransaction.ToArray()[i]);
                    i++;
                    amount += detail.DiscountedAmount;

                }
                    await _pegasusContext.Payment.AddAsync(payment);
                    await _pegasusContext.SaveChangesAsync();
                if(amount != payment.Amount)
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

        private async Task<IActionResult> SaveLesson(int invoice_id)
        {
            var result = new Result<object>();

            var invoice = await _pegasusContext.Invoice.FirstOrDefaultAsync(x => x.InvoiceId == invoice_id);
            var course = await _pegasusContext.One2oneCourseInstance.FirstOrDefaultAsync(x => x.CourseInstanceId == invoice.CourseInstanceId);
            var holiday = await _pegasusContext.Holiday.Select(x => x.HolidayDate).ToArrayAsync();
            var schedules = await _pegasusContext.CourseSchedule.Where(x => x.CourseInstanceId == invoice.CourseInstanceId).ToArrayAsync();
            var amendments = await _pegasusContext.Amendment.Where(x => x.CourseInstanceId == invoice.CourseInstanceId && x.BeginDate <= invoice.EndDate).ToArrayAsync();

            DateTime begindate_invoice = (DateTime)invoice.BeginDate;
            //get the day of week of the begindate in invoice
            int DayOfWeek_invoice = day_trans(begindate_invoice.DayOfWeek.ToString());

            //save the begindate of each lesson[each schedule]
            DateTime[] lesson_begindate = new DateTime[schedules.Length];
            //count the week of course(each lesson)
            int[] num = new int[schedules.Length];

            for (int i = 0; i < invoice.LessonQuantity;)
            {
                int lesson_flag = 0;

                foreach (var schedule in schedules)
                {
                    int flag = 0;
                    int count = 0; //count the day between invoice begindate and course date

                    //calculated the begindate of the course
                    if (DayOfWeek_invoice >= (int)schedule.DayOfWeek)
                    {
                        count = (int)(7 - DayOfWeek_invoice + schedule.DayOfWeek);
                        lesson_begindate[lesson_flag] = begindate_invoice.AddDays(count + 7 * num[lesson_flag]);
                    }
                    else if (DayOfWeek_invoice < (int)schedule.DayOfWeek)
                    {
                        count = (int)(schedule.DayOfWeek - DayOfWeek_invoice);
                        lesson_begindate[lesson_flag] = begindate_invoice.AddDays(count + 7 * num[lesson_flag]);
                    }

                    lesson_begindate[lesson_flag] = Convert.ToDateTime(lesson_begindate[lesson_flag].ToString("yyyy-MM-dd"));

                    //begin to generate the lesson
                    try
                    {
                        Lesson lesson = new Lesson();
                        lesson.CourseInstanceId = invoice.CourseInstanceId;
                        lesson.CreatedAt = DateTime.Now;
                        lesson.RoomId = course.RoomId;
                        lesson.OrgId = (short)course.OrgId;
                        lesson.TeacherId = course.TeacherId;
                        lesson.LearnerId = (int)invoice.LearnerId;
                        lesson.InvoiceId = invoice.InvoiceId;

                        string begintime = "";
                        string endtime = "";

                        //if the lesson has been motified
                        if (amendments != null)
                        {
                            foreach (var amendment in amendments)
                            {
                                if (lesson_begindate[lesson_flag] >= amendment.BeginDate && lesson_begindate[lesson_flag] <= amendment.EndDate)
                                {
                                    if (amendment.AmendType == 1)
                                    {
                                        while (lesson_begindate[lesson_flag] <= amendment.EndDate)
                                        {
                                            num[lesson_flag]++;
                                            lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(7);
                                        }

                                    }

                                    else if (amendment.AmendType == 2 && schedule.CourseScheduleId == amendment.CourseScheduleId)
                                    {
                                        count = 0;
                                        if (schedule.DayOfWeek > (int)amendment.DayOfWeek)
                                        {
                                            count = (int)(7 - schedule.DayOfWeek + amendment.DayOfWeek);
                                            lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(count);
                                        }
                                        else if (schedule.DayOfWeek <= (int)amendment.DayOfWeek)
                                        {
                                            count = (int)(amendment.DayOfWeek - schedule.DayOfWeek);
                                            lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(count);
                                        }
                                        begintime = amendment.BeginTime.ToString();
                                        endtime = amendment.EndTime.ToString();
                                        flag = 1;
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
                                    num[lesson_flag]++;
                                    lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(7);
                                }
                            }
                        }

                        //if the first lesson of the week(datetime) earlier than the lesson of last week, generate this lesson and break the loop (otherlesson of this week waiting for the first lesson)
                        if (lesson_flag == 0 && i != 0)
                        {
                            DateTime max = lesson_begindate[lesson_flag];
                            foreach (DateTime date in lesson_begindate)
                            {
                                if (DateTime.Compare(date, max) > 0)
                                {
                                    flag = 2;
                                    break;
                                }

                            }
                        }

                        if (lesson_begindate[lesson_flag] > invoice.EndDate)
                        {
                            i = (int)invoice.LessonQuantity;
                            break;
                        }

                        string lesson_begindate_result = lesson_begindate[lesson_flag].ToString("yyyy-MM-dd");
                        //Concat the datetime, date from invoice and time from schedule
                        if (flag == 0 || flag==2)
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
                        await _pegasusContext.Lesson.AddAsync(lesson);
                        //await _pegasusContext.SaveChangesAsync();

                    }
                    catch (Exception e)
                    {
                        result.ErrorMessage = e.Message;
                        result.IsSuccess = false;
                        result.IsFound = false;
                        return BadRequest(result);
                    }
                    i++;
                    if (i >= invoice.LessonQuantity) break;
                    num[lesson_flag]++;
                    if (flag == 2) break;
                    lesson_flag++;
                }
            }
            await _pegasusContext.SaveChangesAsync();
            return Ok(result);
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

    }
}