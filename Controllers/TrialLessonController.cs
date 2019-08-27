using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrialLessonController : BasicController
    {
        private readonly IMapper _mapper;
        public TrialLessonController(ablemusicContext ablemusicContext, IMapper mapper, ILogger<TrialLessonController> log) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }
        [HttpGet("[action]/{orgId}/{roomId}/{startTime}")]
        [CheckModelFilter]
        public async Task<IActionResult> GetTeacher(short orgId, short roomId, DateTime startTime)
        {
            var result = new Result<object>();
            try
            {
                var lessons = _ablemusicContext.Lesson.Include(l => l.Teacher)
                    .Where(st => st.BeginTime.Value.Date == startTime.Date && st.OrgId == orgId && st.RoomId == roomId && st.IsCanceled != 1)
                    .Select(l => new
                    {
                        l.TeacherId,
                        l.Teacher.FirstName,
                        l.Teacher.LastName,
                        l.BeginTime,
                        diff = Math.Abs(l.BeginTime.Value.Subtract(startTime).TotalMinutes)
                    }).OrderBy(l => l.diff);
                var tmpLesson1 = await (from l in lessons
                                        group l by l.TeacherId into g
                                        select new
                                        {
                                            TeacherId = g.Key,
                                            FirstName = g.Max(l => l.FirstName),
                                            LastName = g.Max(l => l.LastName),
                                            diff = g.Min(l => l.diff)
                                        }).OrderBy(l => l.diff).ToListAsync();
                if (tmpLesson1.Count > 0)
                {
                    result.Data = tmpLesson1;
                    return Ok(result);
                }


                short dayOfWeek = (short)startTime.DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7;
                var tmpLesson2 = await _ablemusicContext.AvailableDays.Include(a => a.Teacher)
                .Where(a => a.DayOfWeek == dayOfWeek && a.OrgId == orgId && a.RoomId == roomId && a.Teacher.IsActivate == 1)
                .Select(a => new { a.TeacherId, a.Teacher.FirstName, a.Teacher.LastName }).ToListAsync();

                if (tmpLesson2.Count > 0)
                {
                    result.Data = tmpLesson2;
                    return Ok(result);
                }

                var tmpLesson3 = await _ablemusicContext.AvailableDays.Include(a => a.Teacher)
                .Where(a => a.DayOfWeek == dayOfWeek && a.OrgId == orgId && a.Teacher.IsActivate ==1)
                .Select(a => new { a.TeacherId, a.Teacher.FirstName, a.Teacher.LastName }).Distinct().ToListAsync();
                if (tmpLesson3.Count > 0)
                {
                    result.Data = tmpLesson3;
                    return Ok(result);
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
        // POST: api/TrialLesson
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> Post([FromBody] TrialLessonViewModel trialLessonViewModel)
        {
            var result = new Result<Lesson>();
            var lesson = new Lesson();
            var payment = new Payment();
            var invoiceWaiting = new InvoiceWaitingConfirm();
            var invoice = new Invoice();

            lesson.LearnerId = trialLessonViewModel.LearnerId;
            lesson.RoomId = trialLessonViewModel.RoomId;
            lesson.TeacherId = trialLessonViewModel.TeacherId;
            lesson.OrgId = trialLessonViewModel.OrgId;
            lesson.IsCanceled = 0;
            lesson.Reason = null;
            lesson.CreatedAt = toNZTimezone(DateTime.UtcNow);
            lesson.CourseInstanceId = null;
            lesson.GroupCourseInstanceId = null;
            lesson.IsTrial = 1;
            lesson.BeginTime = trialLessonViewModel.BeginTime;
            lesson.EndTime = trialLessonViewModel.EndTime;
            lesson.IsChanged = 0;
            lesson.IsConfirm = 0;
            lesson.TrialCourseId = trialLessonViewModel.TrialCourseId;
            lesson.IsPaid = trialLessonViewModel.IsPayNow ? (short)1 : (short)0;

            payment.PaymentMethod = trialLessonViewModel.PaymentMethod;
            payment.LearnerId = trialLessonViewModel.LearnerId;
            payment.Amount = trialLessonViewModel.Amount;
            payment.CreatedAt = toNZTimezone(DateTime.UtcNow);
            payment.StaffId = trialLessonViewModel.StaffId;
            payment.InvoiceId = null;
            payment.BeforeBalance = null;
            payment.AfterBalance = null;
            payment.PaymentType = 3;
            payment.IsConfirmed = 0;
            payment.Comment = null;

            invoiceWaiting.LessonFee = trialLessonViewModel.Amount;
            invoiceWaiting.LearnerId = trialLessonViewModel.LearnerId;
            invoiceWaiting.LearnerName = await _ablemusicContext.Learner.
                        Where(l => l.LearnerId == trialLessonViewModel.LearnerId).Select(l => l.FirstName).FirstOrDefaultAsync();
            invoiceWaiting.BeginDate = trialLessonViewModel.BeginTime.Value.Date;
            invoiceWaiting.EndDate = trialLessonViewModel.BeginTime.Value.Date;
            invoiceWaiting.TotalFee = trialLessonViewModel.Amount;
            invoiceWaiting.DueDate = trialLessonViewModel.BeginTime.Value.Date;
            invoiceWaiting.PaidFee = 0;
            invoiceWaiting.OwingFee = trialLessonViewModel.Amount;
            invoiceWaiting.CreatedAt = toNZTimezone(DateTime.UtcNow);
            invoiceWaiting.IsPaid = 0;
            invoiceWaiting.TermId = await _ablemusicContext.Term.
                        Where(t => t.BeginDate <= trialLessonViewModel.BeginTime.Value &&
                                t.EndDate >= trialLessonViewModel.BeginTime.Value
                         ).Select(l => l.TermId).FirstOrDefaultAsync();
            invoiceWaiting.LessonQuantity = 1;
            invoiceWaiting.CourseName = "Trial Lesson";
            invoiceWaiting.IsConfirmed = 1;
            invoiceWaiting.IsEmailSent = 0;
            invoiceWaiting.IsActivate = 1;

            List<Lesson> conflictRooms = new List<Lesson>();
            List<Lesson> conflictTeacherLessons = new List<Lesson>();
            DateTime beginTime = lesson.BeginTime.Value.AddMinutes(-60);
            DateTime endTime = lesson.EndTime.Value.AddMinutes(60);
            var lessonConflictCheckerService = new LessonConflictCheckerService(_ablemusicContext, lesson);
            Result<List<object>> lessonConflictCheckResult;
            try
            {
                lessonConflictCheckResult = await lessonConflictCheckerService.CheckBothRoomAndTeacher();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (!lessonConflictCheckResult.IsSuccess)
            {
                return BadRequest(lessonConflictCheckResult);
            }

            try
            {
                await _ablemusicContext.Lesson.AddAsync(lesson);
                if (trialLessonViewModel.IsPayNow)
                    await _ablemusicContext.Payment.AddAsync(payment);
                else
                {
                    await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoiceWaiting);
                    invoiceWaiting.InvoiceNum = invoiceWaiting.WaitingId.ToString();
                    _mapper.Map(invoiceWaiting, invoice);
                    invoice.IsActive = 1;
                    await _ablemusicContext.Invoice.AddAsync(invoice);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = lesson;
            return Ok(result);
        }

        [HttpPost("[action]")]
        [CheckModelFilter]
        public async Task<IActionResult> RegistTrial([FromBody] TrialLessonAndLearnerModel trialLessonViewModel)
        {
            var result = new Result<Lesson>();
            var lesson = new Lesson();
            var payment = new Payment();
            var learner = new Learner();
            var invoiceWaiting = new InvoiceWaitingConfirm();
            var invoice = new Invoice();
            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                learner.FirstName = trialLessonViewModel.FirstName;
                learner.LastName = trialLessonViewModel.LastName;
                learner.ContactNum = trialLessonViewModel.ContactNum;
                learner.Email = trialLessonViewModel.Email;
                learner.EnrollDate = toNZTimezone(DateTime.UtcNow);
                learner.OrgId = trialLessonViewModel.OrgId;
                learner.CreatedAt = toNZTimezone(DateTime.UtcNow);
                learner.IsActive = 1;
                //learner.LearnerLevel = 0;
                learner.PaymentPeriod = 1;
               // learner.LevelType = 1;
                _ablemusicContext.Add(learner);
                await _ablemusicContext.SaveChangesAsync();

                                    // var fundItem = new Fund { Balance = 0, LearnerId = newLearner.LearnerId };
                _ablemusicContext.Add(new Fund { Balance = 0, LearnerId = learner.LearnerId });
                await _ablemusicContext.SaveChangesAsync();
                
                _ablemusicContext.Add( new User
                {
                    UserName = learner.Email,
                    Password = "helloworld",
                    CreatedAt = toNZTimezone(DateTime.UtcNow),
                    RoleId = 4,
                    IsActivate = 1
                });
                await _ablemusicContext.SaveChangesAsync();

                lesson.LearnerId = learner.LearnerId;
                lesson.RoomId = trialLessonViewModel.RoomId;
                lesson.TeacherId = trialLessonViewModel.TeacherId;
                lesson.OrgId = trialLessonViewModel.OrgId;
                lesson.IsCanceled = 0;
                lesson.Reason = null;
                lesson.CreatedAt = toNZTimezone(DateTime.UtcNow);
                lesson.CourseInstanceId = null;
                lesson.GroupCourseInstanceId = null;
                lesson.IsTrial = 1;
                lesson.BeginTime = trialLessonViewModel.BeginTime;
                lesson.EndTime = trialLessonViewModel.EndTime;
                lesson.IsChanged = 0;
                lesson.IsConfirm = 0;
                lesson.TrialCourseId = trialLessonViewModel.TrialCourseId;
                lesson.IsPaid = trialLessonViewModel.IsPayNow ? (short)1 : (short)0;

                payment.PaymentMethod = trialLessonViewModel.PaymentMethod;
                payment.LearnerId = learner.LearnerId;
                payment.Amount = trialLessonViewModel.Amount;
                payment.CreatedAt = toNZTimezone(DateTime.UtcNow);
                payment.StaffId = trialLessonViewModel.StaffId;
                payment.InvoiceId = null;
                payment.BeforeBalance = null;
                payment.AfterBalance = null;
                payment.PaymentType = 3;
                payment.IsConfirmed = 0;
                payment.Comment = null;

                invoiceWaiting.LessonFee = trialLessonViewModel.Amount;
                invoiceWaiting.LearnerId = learner.LearnerId;
                invoiceWaiting.LearnerName = await _ablemusicContext.Learner.
                            Where(l => l.LearnerId == learner.LearnerId).Select(l => l.FirstName).FirstOrDefaultAsync();
                invoiceWaiting.BeginDate = trialLessonViewModel.BeginTime.Value.Date;
                invoiceWaiting.EndDate = trialLessonViewModel.BeginTime.Value.Date;
                invoiceWaiting.TotalFee = trialLessonViewModel.Amount;
                invoiceWaiting.DueDate = trialLessonViewModel.BeginTime.Value.Date;
                invoiceWaiting.PaidFee = 0;
                invoiceWaiting.OwingFee = trialLessonViewModel.Amount;
                invoiceWaiting.CreatedAt = toNZTimezone(DateTime.UtcNow);
                invoiceWaiting.IsPaid = 0;
                invoiceWaiting.TermId = await _ablemusicContext.Term.
                            Where(t => t.BeginDate <= trialLessonViewModel.BeginTime.Value &&
                                    t.EndDate >= trialLessonViewModel.BeginTime.Value
                             ).Select(l => l.TermId).FirstOrDefaultAsync();
                invoiceWaiting.LessonQuantity = 1;
                invoiceWaiting.CourseName = "Trial Lesson";
                invoiceWaiting.IsConfirmed = 1;
                invoiceWaiting.IsEmailSent = 0;
                invoiceWaiting.IsActivate = 1;

                List<Lesson> conflictRooms = new List<Lesson>();
                List<Lesson> conflictTeacherLessons = new List<Lesson>();
                DateTime beginTime = lesson.BeginTime.Value.AddMinutes(-60);
                DateTime endTime = lesson.EndTime.Value.AddMinutes(60);
                var lessonConflictCheckerService = new LessonConflictCheckerService(_ablemusicContext, lesson);
                Result<List<object>> lessonConflictCheckResult;
                try
                {
                    lessonConflictCheckResult = await lessonConflictCheckerService.CheckBothRoomAndTeacher();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
                if (!lessonConflictCheckResult.IsSuccess)
                {
                    return BadRequest(lessonConflictCheckResult);
                }

                try
                {
                    await _ablemusicContext.Lesson.AddAsync(lesson);
                    if (trialLessonViewModel.IsPayNow)
                        await _ablemusicContext.Payment.AddAsync(payment);
                    else
                    {
                        await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoiceWaiting);
                        invoiceWaiting.InvoiceNum = invoiceWaiting.WaitingId.ToString();
                        _mapper.Map(invoiceWaiting, invoice);
                        invoice.IsActive = 1;
                        await _ablemusicContext.Invoice.AddAsync(invoice);
                    }
                    await _ablemusicContext.SaveChangesAsync();
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {   
                    dbContextTransaction.Rollback();
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
                result.Data = lesson;
                return Ok(result);
            }
        }
    }
}
