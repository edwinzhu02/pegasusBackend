using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Utilities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace Pegasus_backend.Services
{
    public class GroupCourseGenerateService
    {
        private readonly ablemusicContext _ablemusicContext;
        private readonly ILogger<object> _log;

        public GroupCourseGenerateService(ablemusicContext ablemusicContext, ILogger<object> log)
        {
            _ablemusicContext = ablemusicContext;
            _log = log;
        }

        public async Task<Result<string>> GenerateLessons(int termId)
        {
            var result = new Result<string>();
            List<GroupCourseInstance> groupCourseInstances;
            Term term;
            List<Holiday> holidays;

            try
            {
                term = await _ablemusicContext.Term.Where(t => t.TermId == termId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            if (term == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Term not found";
                return result;
            }
            try
            {
                groupCourseInstances = await _ablemusicContext.GroupCourseInstance.Where(gc => gc.IsActivate == 1).Include(gc => gc.CourseSchedule).ToListAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            if (groupCourseInstances.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "GroupCourseInstance not found";
                return result;
            }

            List<Lesson> lessonsTobeAppend = new List<Lesson>();

            foreach (var groupCourseInstance in groupCourseInstances)
            {
                if (groupCourseInstance.CourseSchedule.Count > 0)
                {
                    var courseSchedules = groupCourseInstance.CourseSchedule;
                    DateTime begin = groupCourseInstance.InvoiceDate.HasValue ? groupCourseInstance.InvoiceDate.Value : groupCourseInstance.BeginDate.Value;
                    DateTime end = groupCourseInstance.EndDate.Value;
                    begin = begin > term.BeginDate ? begin : term.BeginDate.Value;
                    end = end > term.EndDate ? term.EndDate.Value : end;
                    if (begin.Date < end.Date)
                    {
                        foreach (var schedule in courseSchedules)
                        {
                            DateTime currentDate = begin;
                            int currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
                            while (currentDayOfWeek != schedule.DayOfWeek)
                            {
                                currentDate = currentDate.AddDays(1);
                                currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
                            }
                            while (currentDate <= end)
                            {
                                bool isOnHoliday = false;
                                foreach (var h in holidays)
                                {
                                    if (h.HolidayDate.Date == currentDate.Date)
                                    {
                                        isOnHoliday = true;
                                    }
                                }
                                if (!isOnHoliday)
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
                                        InvoiceNum = null,  
                                        IsConfirm = 0,
                                        TrialCourseId = null,
                                        IsChanged = 0
                                    });
                                }
                                currentDate = currentDate.AddDays(7);
                                currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
                            }
                        }
                    }
                    groupCourseInstance.InvoiceDate = end.Date;
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
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }

            result.Data = lessonsTobeAppend.Count.ToString() + " Lessons has been arranged successfully";

            return result;
        }
    }
}
