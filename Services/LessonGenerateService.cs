using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.Services;
using AutoMapper;
using Pegasus_backend.Utilities;


namespace Pegasus_backend.Services
{
    public class LessonGenerateService
    {

        private readonly ablemusicContext _ablemusicContext;
        private readonly IMapper _mapper;
        private readonly DateTime _today;

        public LessonGenerateService(ablemusicContext ablemusicContext, IMapper mapper)
        {
            _ablemusicContext = ablemusicContext;
            _mapper = mapper;
           _today = DateTime.UtcNow.ToNZTimezone();
        }


        //auto-generate lessons sort by invoice when the invoice is paid
        public async Task<int> SaveLesson(int invoice_id, int isWaitingConfirm, int isOne2one)
        {
            var result = new Result<object>();
            var invoice = new WaitingOrInvoice();
            if (isWaitingConfirm == 1)
            {
                var tem = await _ablemusicContext.InvoiceWaitingConfirm.FirstOrDefaultAsync(x => x.WaitingId == invoice_id);
                _mapper.Map(tem, invoice);
            }
            else
            {
                var tem = await _ablemusicContext.Invoice.FirstOrDefaultAsync(x => x.InvoiceId == invoice_id);
                _mapper.Map(tem, invoice);
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
                        lesson.CreatedAt = _today;
                        lesson.RoomId = course.RoomId;
                        lesson.OrgId = (short)course.OrgId;
                        lesson.TeacherId = course.TeacherId;
                        lesson.LearnerId = (int)invoice.LearnerId;
                        lesson.InvoiceId = invoice.InvoiceId;
                        lesson.IsConfirm = 0;
                        lesson.IsCanceled = 0;
                        lesson.IsChanged = 0;
                        lesson.IsTrial = 0;
                        lesson.IsPaid = 1;

                        string begintime = "";
                        string endtime = "";

                        //if the lesson has been motified
                        if (amendments != null)
                        {
                            int flag_DOW = 0;
                            int amend_conflict = 0;

                            foreach (var amendment in amendments)
                            {

                                if (lesson_begindate[lesson_flag] >= amendment.BeginDate && (lesson_begindate[lesson_flag] <= amendment.EndDate || amendment.EndDate == null))
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

                                    else if (amendment.AmendType == 2 && schedule.CourseScheduleId == amendment.CourseScheduleId && amend_conflict == 0)
                                    {
                                        count = 0;
                                        count = (int)amendment.DayOfWeek - (int)schedule.DayOfWeek;
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
                                    else if (amendment.AmendType == 2 && schedule.CourseScheduleId == amendment.CourseScheduleId && amend_conflict != 0)
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
                        if (isOne2one == 1)
                        {
                            await _ablemusicContext.Lesson.AddAsync(lesson);
                            await _ablemusicContext.SaveChangesAsync();
                        }

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


        public async Task<Result<IActionResult>> Generateone2oneInvoice(int term_id, int instance_id = 0)
        {
            var result = new Result<IActionResult>();
            try
            {
                //get concert fee configuraton
                var concertFeeName = _ablemusicContext.Lookup.
                    Where(x => x.LookupType == 15 && x.PropValue == 1).Select(x => x.PropName).FirstOrDefault(); ;

                string concertFeeStr = _ablemusicContext.Lookup.
                        Where(x => x.LookupType == 15 && x.PropValue == 2).Select(x => x.PropName).FirstOrDefault();
                int concertFee = Int32.Parse(concertFeeStr);
                //get note fee configuraton
                string noteFeeName = _ablemusicContext.Lookup.
                        Where(x => x.LookupType == 16 && x.PropValue == 1).Select(x => x.PropName).FirstOrDefault();
                string noteFeeStr = _ablemusicContext.Lookup.
                        Where(x => x.LookupType == 16 && x.PropValue == 2).Select(x => x.PropName).FirstOrDefault();
                int noteFee = Int32.Parse(noteFeeStr);
                //get extra fee configuraton
                string extraFeeStr = _ablemusicContext.Lookup.
                        Where(x => x.LookupType == 17 && x.PropValue == 1).Select(x => x.PropName).FirstOrDefault();
                int extraFee = Int32.Parse(extraFeeStr);

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
                //int i = 0;
                foreach (var course_instance in course_instances)
                {
                    if (course_instance.InvoiceDate >= Convert.ToDateTime(term.EndDate)) continue;
                    InvoiceWaitingConfirm invoice = new InvoiceWaitingConfirm();

                    invoice.LearnerId = course_instance.LearnerId;
                    invoice.LearnerName = course_instance.Learner.FirstName;
                    invoice.CourseInstanceId = course_instance.CourseInstanceId;
                    invoice.CourseName = course_instance.Course.CourseName;
                    invoice.ConcertFeeName = concertFeeName;
                    invoice.ConcertFee = concertFee;
                    invoice.LessonNoteFeeName = noteFeeName;
                    invoice.NoteFee = noteFee;
                    invoice.TermId = (short)term_id;
                    invoice.IsPaid = 0;
                    invoice.PaidFee = 0;
                    invoice.CreatedAt = _today;
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
                        //using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                        //{
                        //    lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 1);
                        //    dbContextTransaction.Rollback();

                        //}
                        int isExist = IsLearnerHasPayExtreFee((int)invoice.TermId, (int)invoice.LearnerId);
                        if (isExist == 1)
                        {
                            //invoice.ConcertFeeName = concertFeeName;
                            invoice.ConcertFee = 0;
                            //invoice.LessonNoteFeeName = noteFeeName;
                            invoice.NoteFee = 0;
                        }
                        lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 1);
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

                        int isExist = IsLearnerHasPayExtreFee((int)invoice.TermId, (int)invoice.LearnerId);
                        if (isExist == 1)
                        {
                            //invoice.ConcertFeeName = concertFeeName;
                            invoice.ConcertFee = 0;
                            //invoice.LessonNoteFeeName = noteFeeName;
                            invoice.NoteFee = 0;
                        }
                        await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoice);
                        await _ablemusicContext.SaveChangesAsync();
                        //using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                        //{
                        //    lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 1);
                        //    dbContextTransaction.Rollback();

                        //}
                        lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 1);
                    }
                    if (invoice.BeginDate != null) invoice.DueDate = Convert.ToDateTime(invoice.BeginDate).AddDays(-1);
                    invoice.LessonFee = course_instance.Course.Price * lesson_quantity;
                    if (course_instance.Learner.PaymentPeriod == 2)
                        invoice.LessonFee = invoice.LessonFee + extraFee;
                    invoice.LessonFee = course_instance.Course.Price * lesson_quantity;
                    invoice.OwingFee = invoice.LessonFee;
                    invoice.TotalFee = invoice.LessonFee;
                    invoice.LessonQuantity = lesson_quantity;
                    if (invoice.LessonFee <= 0) continue;
                    _ablemusicContext.InvoiceWaitingConfirm.Update(invoice);
                    invoice.InvoiceNum = invoice.WaitingId.ToString();
                    _ablemusicContext.Update(courseIns);

                    await _ablemusicContext.SaveChangesAsync();

                    //i++;
                    //if (i == 4) break;

                }
                //result.Data = i;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            return result;
        }

        public async Task<Result<IActionResult>> GenerateGroupInvoice(int term_id, int instance_id = 0)
        {
            var concertFeeName = _ablemusicContext.Lookup.
                    Where(x => x.LookupType == 15 && x.PropValue == 1).Select(x => x.PropName).FirstOrDefault(); ;

            string concertFeeStr = _ablemusicContext.Lookup.
                    Where(x => x.LookupType == 15 && x.PropValue == 2).Select(x => x.PropName).FirstOrDefault();
            int concertFee = Int32.Parse(concertFeeStr);
            //get note fee configuraton
            string noteFeeName = _ablemusicContext.Lookup.
                    Where(x => x.LookupType == 16 && x.PropValue == 1).Select(x => x.PropName).FirstOrDefault();
            string noteFeeStr = _ablemusicContext.Lookup.
                    Where(x => x.LookupType == 16 && x.PropValue == 2).Select(x => x.PropName).FirstOrDefault();
            int noteFee = Int32.Parse(noteFeeStr);
            //get extra fee configuraton
            string extraFeeStr = _ablemusicContext.Lookup.
                    Where(x => x.LookupType == 17 && x.PropValue == 1).Select(x => x.PropName).FirstOrDefault();
            int extraFee = Int32.Parse(extraFeeStr);

            var result = new Result<IActionResult>();
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
                    CourseName = x.Course.CourseName,
                    Price = x.Course.Price,
                    Learners = x.LearnerGroupCourse.Select(s => new { s.Learner.FirstName, s.LearnerId, s.CreatedAt, s.BeginDate, s.EndDate, s.InvoiceDate, s.LearnerGroupCourseId, s.IsActivate }).Where(s => s.IsActivate == 1).ToArray()

                })
                .ToListAsync();
            if (instance_id != 0)
            {
                group_course_instances = group_course_instances.Where(x => x.GroupCourseInstanceId == instance_id).ToList();
            }

            var term = await _ablemusicContext.Term.FirstOrDefaultAsync(x => x.TermId == term_id);
            //int i = 0;
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
                    invoice.CreatedAt = _today;
                    invoice.IsConfirmed = 0;
                    invoice.IsActivate = 1;
                    invoice.IsEmailSent = 0;
                    invoice.ConcertFeeName = concertFeeName;
                    invoice.ConcertFee = concertFee;
                    invoice.LessonNoteFeeName = noteFeeName;
                    invoice.NoteFee = noteFee;

                    var courseIns = await _ablemusicContext.LearnerGroupCourse.FirstOrDefaultAsync(x => x.LearnerGroupCourseId == learner.LearnerGroupCourseId);
                    int lesson_quantity = 0;

                    if (learner.InvoiceDate == null || (learner.InvoiceDate < term.EndDate && learner.BeginDate <= term.EndDate))
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
                        //using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                        //{
                        //    lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 0);
                        //    dbContextTransaction.Rollback();
                        //}

                        lesson_quantity = await SaveLesson(invoice.WaitingId, 1, 0);
                        courseIns.InvoiceDate = invoice.EndDate;
                    }


                    if (invoice.BeginDate != null) invoice.DueDate = Convert.ToDateTime(invoice.BeginDate).AddDays(-1);
                    invoice.LessonFee = group_course_instance.Price * lesson_quantity;

                    invoice.OwingFee = invoice.LessonFee;
                    invoice.TotalFee = invoice.LessonFee;
                    invoice.LessonQuantity = lesson_quantity;
                    if (invoice.LessonFee <= 0) continue;
                    int isExist = IsLearnerHasPayExtreFee((int)invoice.TermId, (int)invoice.LearnerId);
                    if (isExist == 1)
                    {
                        //invoice.ConcertFeeName = concertFeeName;
                        invoice.ConcertFee = 0;
                        //invoice.LessonNoteFeeName = noteFeeName;
                        invoice.NoteFee = 0;
                    }
                    invoice.InvoiceNum = invoice.WaitingId.ToString();
                    _ablemusicContext.InvoiceWaitingConfirm.Update(invoice);
                    _ablemusicContext.Update(courseIns);

                    await _ablemusicContext.SaveChangesAsync();

                    //i++;
                    //j++;
                    //if (i == 5) break;
                }
                //if (j > 1) break;
            }
            //result.Data = i;

            return result;

        }


        public void UpdateWaitingInvoicetoActive()
        {
            List<InvoiceWaitingConfirm> InvoiceWaitingConfirms = new List<InvoiceWaitingConfirm>();
            InvoiceWaitingConfirms = _ablemusicContext.InvoiceWaitingConfirm.Where(i => i.IsActivate == 3).ToList();
            foreach (var InvoiceWaitingConfirm in InvoiceWaitingConfirms)
            {
                TimeSpan ts = (DateTime)InvoiceWaitingConfirm.BeginDate - _today;
                int days = ts.Days;

                if ((InvoiceWaitingConfirm.CourseInstanceId != null && days <= 30) || (InvoiceWaitingConfirm.GroupCourseInstanceId != null && days <= 14))
                {
                    InvoiceWaitingConfirm.IsActivate = 1;
                    _ablemusicContext.Update(InvoiceWaitingConfirm);
                    _ablemusicContext.SaveChanges();
                }
            }

        }

        public int IsLearnerHasPayExtreFee(int term_id, int learner_id)
        {
            var term = _ablemusicContext.Term.Where(x => x.TermId == term_id).FirstOrDefault();
            var learnerWaitingInvoice = _ablemusicContext.InvoiceWaitingConfirm.Where(x => x.BeginDate >= term.BeginDate && x.EndDate <= term.EndDate && x.LearnerId == learner_id).ToList();
            if (learnerWaitingInvoice.Count == 0)
            {
                return 0;
            }
            return 1;
        }


        public async Task GetTerm(DateTime date, int instance_id=0,int isone2one=3)
        {
            //string time DateTime time
            var terms = _ablemusicContext.Term.OrderBy(x => x.BeginDate).ToArray();
            DateTime start_date = new DateTime();

            //learn course_instance begin_date
            start_date = DateTime.Parse(date.ToString());

            foreach (var term in terms)
            {
                DateTime BeginDate = new DateTime();
                BeginDate = DateTime.Parse(term.BeginDate.ToString());

                TimeSpan ts = BeginDate.Subtract(start_date);

                int day = ts.Days;
                if (isone2one == 3)
                {
                    if (day >= 0 && day <= 30)
                    {

                        int next_term = term.TermId;
                        await Generateone2oneInvoice(next_term, instance_id);
                        await GenerateGroupInvoice(next_term, instance_id);
                    }

                    if (start_date >= term.BeginDate && start_date <= term.EndDate)
                    {
                        int this_term = term.TermId;
                        await Generateone2oneInvoice(this_term, instance_id);
                        await GenerateGroupInvoice(this_term, instance_id);
                    }
                }
                if (isone2one == 0)
                {
                    if (day >= 0 && day <= 30)
                    {

                        int next_term = term.TermId;
                        await GenerateGroupInvoice(next_term, instance_id);
                    }

                    if (start_date >= term.BeginDate && start_date <= term.EndDate)
                    {
                        int this_term = term.TermId;
                        await GenerateGroupInvoice(this_term, instance_id);
                    }
                }
                if (isone2one == 1)
                {
                    if (day >= 0 && day <= 30)
                    {

                        int next_term = term.TermId;
                        await Generateone2oneInvoice(next_term, instance_id);
                    }

                    if (start_date >= term.BeginDate && start_date <= term.EndDate)
                    {
                        int this_term = term.TermId;
                        await Generateone2oneInvoice(this_term, instance_id);
                    }
                }

            }
            UpdateWaitingInvoicetoActive();
        }


    }
}

