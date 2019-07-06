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
            lesson.InvoiceId = null;
            lesson.IsConfirm = 0;
            lesson.TrialCourseId = trialLessonViewModel.TrialCourseId;

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

            invoiceWaiting.LessonFee =  trialLessonViewModel.Amount;
            invoiceWaiting.LearnerId = trialLessonViewModel.LearnerId;
            invoiceWaiting.LearnerName = await _ablemusicContext.Learner.
                        Where(l => l.LearnerId == trialLessonViewModel.LearnerId  ).Select(l=> l.FirstName).FirstOrDefaultAsync();
            invoiceWaiting.BeginDate = trialLessonViewModel.BeginTime.Value.Date;
            invoiceWaiting.EndDate = trialLessonViewModel.BeginTime.Value.Date;
            invoiceWaiting.TotalFee = trialLessonViewModel.Amount;
            invoiceWaiting.DueDate = trialLessonViewModel.BeginTime.Value.Date;           
            invoiceWaiting.PaidFee = 0;            
            invoiceWaiting.OwingFee =trialLessonViewModel.Amount;                       
            invoiceWaiting.CreatedAt =toNZTimezone(DateTime.UtcNow);      
            invoiceWaiting.IsPaid =0;  
            invoiceWaiting.TermId =await _ablemusicContext.Term.
                        Where(t => t.BeginDate <= trialLessonViewModel.BeginTime.Value &&
                                t.EndDate >= trialLessonViewModel.BeginTime.Value 
                         ).Select(l=> l.TermId).FirstOrDefaultAsync();
            invoiceWaiting.LessonQuantity =1;
            invoiceWaiting.CourseName ="Trial Lesson";
            invoiceWaiting.IsConfirmed = 1;
            invoiceWaiting.IsEmailSent = 0;
            invoiceWaiting.IsActivate = 1;

            List<Lesson> conflictRooms = new List<Lesson>();
            List<Lesson> conflictTeacherLessons = new List<Lesson>();
            DateTime beginTime = lesson.BeginTime.Value.AddMinutes(-60);
            DateTime endTime = lesson.EndTime.Value.AddMinutes(60);
            var lessonConflictCheckerService = new LessonConflictCheckerService(lesson);
            Result<List<object>> lessonConflictCheckResult;
            try
            {
                lessonConflictCheckResult = await lessonConflictCheckerService.CheckBothRoomAndTeacher();
            }
            catch(Exception ex)
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
                else {
                    await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoiceWaiting);
                    invoiceWaiting.InvoiceNum = invoiceWaiting.WaitingId.ToString();
                    _mapper.Map(invoiceWaiting,invoice);
                    invoice.IsActive = 1;
                    await _ablemusicContext.Invoice.AddAsync(invoice);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = lesson;
            return Ok(result);
        }
    }
}
