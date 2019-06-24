using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrialLessonController : BasicController
    {
        public TrialLessonController(ablemusicContext ablemusicContext, ILogger<ValuesController> log) : base(ablemusicContext, log)
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

            List<Lesson> conflictRooms = new List<Lesson>();
            List<Lesson> conflictTeacherLessons = new List<Lesson>();
            DateTime beginTime = lesson.BeginTime.Value.AddMinutes(-60);
            DateTime endTime = lesson.EndTime.Value.AddMinutes(60);
            try
            {
                conflictRooms = await _ablemusicContext.Lesson.Where(l => l.RoomId == lesson.RoomId &&
                    l.OrgId == lesson.OrgId && l.IsCanceled != 1 && l.LessonId != lesson.LessonId &&
                    ((l.BeginTime > lesson.BeginTime && l.BeginTime < lesson.EndTime) ||
                    (l.EndTime > lesson.BeginTime && l.EndTime < lesson.EndTime) ||
                    (l.BeginTime <= lesson.BeginTime && l.EndTime >= lesson.EndTime) ||
                    (l.BeginTime > lesson.BeginTime && l.EndTime < lesson.EndTime)))
                    .ToListAsync();
                conflictTeacherLessons = await _ablemusicContext.Lesson.Where(l => l.TeacherId == lesson.TeacherId &&
                    l.IsCanceled != 1 && l.LessonId != lesson.LessonId &&
                    ((l.BeginTime > beginTime && l.BeginTime < endTime) ||
                    (l.EndTime > beginTime && l.EndTime < endTime) ||
                    (l.BeginTime <= beginTime && l.EndTime >= endTime)))
                    .ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (conflictRooms.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room is not available";
                return BadRequest(result);
            }
            if (conflictTeacherLessons.Count > 0)
            {
                foreach (var c in conflictTeacherLessons)
                {
                    if (c.OrgId != lesson.OrgId ||
                        (c.BeginTime > lesson.BeginTime && c.BeginTime < lesson.EndTime) ||
                        (c.EndTime > lesson.BeginTime && c.EndTime < lesson.EndTime) ||
                        (c.BeginTime <= lesson.BeginTime && c.EndTime >= lesson.EndTime))
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Teacher is not available";
                        return BadRequest(result);
                    }
                }
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
