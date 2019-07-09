using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupCourseArrangeLessonController : BasicController
    {

        public GroupCourseArrangeLessonController(ablemusicContext ablemusicContext, ILogger<PeriodCourseChangeController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/GroupCourseArrangeLesson
        [HttpGet("{groupCourseInstanceId}/{termId}")]
        public async Task<IActionResult> Get(int groupCourseInstanceId, int termId)
        {
            var result = new Result<string>();
            GroupCourseInstance groupCourseInstance;
            Term term;
            List<CourseSchedule> courseSchedules;
            List<Holiday> holidays;

            try
            {
                groupCourseInstance = await _ablemusicContext.GroupCourseInstance.Where(gc => gc.GroupCourseInstanceId == groupCourseInstanceId).FirstOrDefaultAsync();
                term = await _ablemusicContext.Term.Where(t => t.TermId == termId).FirstOrDefaultAsync();
                courseSchedules = await _ablemusicContext.CourseSchedule.Where(cs => cs.GroupCourseInstanceId == groupCourseInstanceId).ToListAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(groupCourseInstance == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "GroupCourseInstance not found";
                return BadRequest(result);
            }
            if(term == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Term not found";
                return BadRequest(result);
            }
            if(courseSchedules.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "CourseSchedule not found";
                return BadRequest(result);
            }

            DateTime begin = groupCourseInstance.InvoiceDate.HasValue ? groupCourseInstance.InvoiceDate.Value : groupCourseInstance.BeginDate.Value;
            DateTime end = groupCourseInstance.EndDate.Value;
            begin = begin > term.BeginDate ? begin : term.BeginDate.Value;
            end = end > term.EndDate ? term.EndDate.Value : end;
            if(begin > end)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "No lesson to arrange in this term";
                return BadRequest(result);
            }

            List<Lesson> lessonsTobeAppend = new List<Lesson>();
            foreach (var schedule in courseSchedules)
            {
                DateTime currentDate = begin;
                int currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
                while(currentDayOfWeek != schedule.DayOfWeek)
                {
                    currentDate = currentDate.AddDays(1);
                    currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
                }
                while(currentDate <= end)
                {
                    lessonsTobeAppend.Add(new Lesson
                    {
                        LearnerId = null,
                        RoomId = groupCourseInstance.RoomId,
                        TeacherId = groupCourseInstance.TeacherId,
                        OrgId = groupCourseInstance.OrgId.Value,
                        IsCanceled = 0,
                        Reason = null,
                        CreatedAt = DateTime.UtcNow.ToNZTimezone(),
                        CourseInstanceId = null,
                        GroupCourseInstanceId = groupCourseInstance.GroupCourseInstanceId,
                        IsTrial = 0,
                        BeginTime = currentDate.Add(schedule.BeginTime.Value),
                        EndTime = currentDate.Add(schedule.EndTime.Value),
                        InvoiceId = null,
                        IsConfirm = 0,
                        TrialCourseId = null,
                        IsChanged = 0
                    });
                    currentDate = currentDate.AddDays(7);
                    currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
                }
            }

            try
            {
                foreach (var lesson in lessonsTobeAppend)
                {
                    await _ablemusicContext.Lesson.AddAsync(lesson);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = lessonsTobeAppend.Count.ToString() + " Lessons has been arranged successfully";

            return Ok(result);
        }
    }
}
