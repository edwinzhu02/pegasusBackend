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
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrialLessonController : BasicController
    {
        public TrialLessonController(ablemusicContext ablemusicContext, ILogger<TrialLessonController> log) : base(ablemusicContext, log)
        {
        }

        // POST: api/TrialLesson
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> Post([FromBody] TrialLessonViewModel trialLessonViewModel)
        {
            var result = new Result<Lesson>();
            var lesson = new Lesson();
            var payment = new Payment();

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
                await _ablemusicContext.Payment.AddAsync(payment);
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
