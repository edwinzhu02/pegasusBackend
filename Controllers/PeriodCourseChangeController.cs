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
using Microsoft.Extensions.Logging;
using Pegasus_backend.Repositories;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodCourseChangeController : BasicController
    {
        private readonly IConfiguration _configuration;

        public PeriodCourseChangeController(ablemusicContext ablemusicContext, ILogger<PeriodCourseChangeController> log, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _configuration = configuration;
        }

        // POST: api/PeriodCourseChange
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostPeriodCourseChange([FromBody] PeriodCourseChangeViewModel inputObj)
        {
            var result = new Result<Amendment>();
            List<Lesson> exsitingLessons;
            Learner learner;
            List<Holiday> holidays;
            List<Amendment> exsitingAmendments;
            Amendment amendment = new Amendment();
            DateTime? todoDateEnd = null;
            DateTime todoDateBegin = inputObj.BeginDate;
            List<Term> terms;
            dynamic courseInfo;
            dynamic newCourseInfo;

            try
            {
                if (inputObj.EndDate.HasValue)
                {
                    exsitingLessons = await _ablemusicContext.Lesson.Where(l => l.LearnerId == inputObj.LearnerId &&
                    l.BeginTime.Value.Date < inputObj.EndDate.Value.Date && l.BeginTime.Value.Date > inputObj.BeginDate.Date&&
                    l.CourseInstanceId == inputObj.InstanceId).ToListAsync();
                } else
                {
                    exsitingLessons = await _ablemusicContext.Lesson.Where(l => l.LearnerId == inputObj.LearnerId &&
                    l.BeginTime.Value.Date > inputObj.BeginDate.Date).ToListAsync();
                }
                terms = await _ablemusicContext.Term.ToListAsync();
                courseInfo = await (from i in _ablemusicContext.One2oneCourseInstance
                                    join cs in _ablemusicContext.CourseSchedule on i.CourseInstanceId equals cs.CourseInstanceId
                                    join l in _ablemusicContext.Learner on i.LearnerId equals l.LearnerId
                                    join t in _ablemusicContext.Teacher on i.TeacherId equals t.TeacherId
                                    join c in _ablemusicContext.Course on i.CourseId equals c.CourseId
                                    join o in _ablemusicContext.Org on i.OrgId equals o.OrgId
                                    join r in _ablemusicContext.Room on i.RoomId equals r.RoomId
                                    where i.CourseInstanceId == inputObj.InstanceId && inputObj.CourseScheduleId == cs.CourseScheduleId
                                    select new
                                    {
                                        OrgId = i.OrgId,
                                        OrgName = o.OrgName,
                                        RoomId = i.RoomId,
                                        RoomName = r.RoomName,
                                        CourseId = i.CourseId,
                                        CourseName = c.CourseName,
                                        c.Duration,
                                        TeacherId = i.TeacherId,
                                        TeacherFirstName = t.FirstName,
                                        TeacherLastName = t.LastName,
                                        TeacherEmail = t.Email,
                                        LearnerId = i.LearnerId,
                                        LearnerFirstName = l.FirstName,
                                        LearnerLastName = l.LastName,
                                        LearnerEmail = l.Email,
                                        DayOfWeek = cs.DayOfWeek,
                                        BeginTime = cs.BeginTime,
                                        EndTime = cs.EndTime,
                                    }).FirstOrDefaultAsync();
                newCourseInfo = new
                {
                    newOrg = await _ablemusicContext.Org.Where(o => o.OrgId == inputObj.OrgId).FirstOrDefaultAsync(),
                    newRoom = await _ablemusicContext.Room.Where(r => r.RoomId == inputObj.RoomId).FirstOrDefaultAsync(),
                };
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == inputObj.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
                exsitingAmendments = await _ablemusicContext.Amendment.Where(a => a.LearnerId == inputObj.LearnerId && a.AmendType == 2 &&
                a.BeginDate == inputObj.BeginDate && a.EndDate == inputObj.EndDate && inputObj.OrgId == a.OrgId && 
                inputObj.BeginTime == a.BeginTime && inputObj.DayOfWeek == a.DayOfWeek && inputObj.InstanceId == a.CourseInstanceId && 
                inputObj.RoomId == a.RoomId && inputObj.IsTemporary == a.IsTemporary).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(exsitingLessons.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Existing lessons are not allowed to change";
                return BadRequest(result);
            }
            if(exsitingAmendments.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "The same change has already being made";
                return BadRequest(result);
            }
            if(courseInfo == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Course instance id not found";
                return BadRequest(result);
            }
            if(newCourseInfo == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Org id or room id not found";
                return BadRequest(result);
            }
            if(!inputObj.EndDate.HasValue && inputObj.IsTemporary == 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "EndDate is required when type is temporary";
                return BadRequest(result);
            }
            switch ((short)courseInfo.Duration)
            {
                case 1: inputObj.EndTime = inputObj.BeginTime.Add(TimeSpan.FromMinutes(30));
                    break;
                case 2: 
                    inputObj.EndTime = inputObj.BeginTime.Add(TimeSpan.FromMinutes(45));
                    break;
                case 3: 
                    inputObj.EndTime = inputObj.BeginTime.Add(TimeSpan.FromMinutes(60));
                    break;
            }

            amendment.CourseInstanceId = inputObj.InstanceId;
            amendment.OrgId = Convert.ToInt16(inputObj.OrgId);
            amendment.DayOfWeek = inputObj.DayOfWeek;
            amendment.BeginTime = inputObj.BeginTime;
            amendment.EndTime = inputObj.EndTime;
            amendment.LearnerId = inputObj.LearnerId;
            amendment.RoomId = inputObj.RoomId;
            amendment.BeginDate = inputObj.BeginDate;
            amendment.EndDate = inputObj.IsTemporary == 1 ? inputObj.EndDate : null;
            amendment.CreatedAt = toNZTimezone(DateTime.UtcNow);
            amendment.Reason = inputObj.Reason;
            amendment.IsTemporary = inputObj.IsTemporary;
            amendment.AmendType = 2;
            amendment.TeacherId = inputObj.TeacherId;
            amendment.CourseScheduleId = inputObj.CourseScheduleId;

            DateTime endDate;
            
            if(inputObj.IsTemporary == 1)
            {
                endDate = inputObj.EndDate.Value;
            }
            else
            {
                Term currentTerm = null;
                Term nextTerm = null;
                DateTime lastDateInAllTerms = inputObj.BeginDate;
                foreach (var t in terms)
                {
                    lastDateInAllTerms = lastDateInAllTerms >= t.EndDate.Value ? lastDateInAllTerms : t.EndDate.Value;
                    if(amendment.BeginDate.Value > t.BeginDate.Value && amendment.BeginDate.Value < t.EndDate.Value)
                    {
                        currentTerm = t;
                    }
                }
                if (currentTerm == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Term not found";
                    return BadRequest(result);
                }
                DateTime currentDay = currentTerm.EndDate.Value.AddDays(1);
                while (currentDay < lastDateInAllTerms)
                {
                    nextTerm = terms.Find(t => t.BeginDate.Value < currentDay && t.EndDate.Value > currentDay);
                    if (nextTerm != null)
                    {
                        break;
                    }
                    else
                    {
                        currentDay = currentDay.AddDays(1);
                    }
                }
                if(nextTerm == null)
                {
                    endDate = currentTerm.EndDate.Value;
                }
                else
                {
                    endDate = nextTerm.EndDate.Value;
                }
            }
            
            var visualLessonsForCheckingConflict = new List<Lesson>();

            DateTime currentDate = amendment.BeginDate.Value;
            int currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
            while (currentDayOfWeek != amendment.DayOfWeek)
            {
                currentDate = currentDate.AddDays(1);
                currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
            }
            while (currentDate <= endDate)
            {
                visualLessonsForCheckingConflict.Add(new Lesson
                {
                    RoomId = amendment.RoomId,
                    TeacherId = amendment.TeacherId,
                    OrgId = amendment.OrgId.Value,
                    BeginTime = currentDate.Add(amendment.BeginTime.Value),
                    EndTime = currentDate.Add(amendment.EndTime.Value),
                });
                currentDate = currentDate.AddDays(7);
                currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
            }

            foreach (var lesson in visualLessonsForCheckingConflict)
            {
                var lessonConflictCheckerService = new LessonConflictCheckerService(lesson);
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
            }
            
            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDateBegin.Date)
                {
                    todoDateBegin = todoDateBegin.AddDays(-1);
                }
            }

            if (inputObj.EndDate.HasValue)
            {
                todoDateEnd = inputObj.EndDate.Value;
                foreach(var holiday in holidays)
                {
                    if (holiday.HolidayDate.Date == todoDateEnd.Value.Date)
                    {
                        todoDateEnd = todoDateEnd.Value.AddDays(-1);
                    }
                }
            }

            TodoRepository todoRepository = new TodoRepository();
            todoRepository.AddSingleTodoList("Period Course Change Remind", TodoListContentGenerator.PeriodCourseChangeForLearner(courseInfo,
                newCourseInfo, inputObj, todoDateBegin), inputObj.UserId, todoDateBegin, null, inputObj.LearnerId, null);
            todoRepository.AddSingleTodoList("Period Course Change Remind", TodoListContentGenerator.PeriodCourseChangeForTeacher(courseInfo,
                newCourseInfo, inputObj, todoDateBegin), inputObj.UserId, todoDateBegin, null, null, courseInfo.TeacherId);
            if (inputObj.EndDate.HasValue && inputObj.IsTemporary == 1)
            {
                todoRepository.AddSingleTodoList("Period Course Change Remind", TodoListContentGenerator.PeriodCourseChangeForLearner(courseInfo,
                    newCourseInfo, inputObj, todoDateEnd.Value), inputObj.UserId, todoDateEnd.Value, null, inputObj.LearnerId, null);
                todoRepository.AddSingleTodoList("Period Course Change Remind", TodoListContentGenerator.PeriodCourseChangeForTeacher(courseInfo,
                    newCourseInfo, inputObj, todoDateEnd.Value), inputObj.UserId, todoDateEnd.Value, null, null, courseInfo.TeacherId);
            }
            var saveTodoResult = await todoRepository.SaveTodoListsAsync();
            if (!saveTodoResult.IsSuccess)
            {
                return BadRequest(saveTodoResult);
            }

            RemindLogRepository remindLogRepository = new RemindLogRepository();
            remindLogRepository.AddSingleRemindLog(courseInfo.LearnerId, courseInfo.LearnerEmail, RemindLogContentGenerator.PeriodCourseChangeForLearner(
                courseInfo, newCourseInfo, inputObj), null, "Period Course Change Remind", null);
            remindLogRepository.AddSingleRemindLog(null, courseInfo.TeacherEmail, RemindLogContentGenerator.PeriodCourseChangeForTeacher(courseInfo,
                newCourseInfo, inputObj), courseInfo.TeacherId, "Period Course Change Remind", null);
            var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
            if (!saveRemindLogResult.IsSuccess)
            {
                return BadRequest(saveRemindLogResult);
            }

            try
            {
                await _ablemusicContext.Amendment.AddAsync(amendment);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            //sending Email
            List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
            foreach (var remind in saveRemindLogResult.Data)
            {
                string currentPersonName;
                if (remind.TeacherId == null)
                {
                    currentPersonName = courseInfo.LearnerFirstName + " " + courseInfo.LearnerLastName;
                }
                else
                {
                    currentPersonName = courseInfo.TeacherFirstName + " " + courseInfo.TeacherLastName;
                }
                string mailContent = EmailContentGenerator.PeriodCourseChange(currentPersonName, courseInfo, newCourseInfo, inputObj);
                notifications.Add(new NotificationEventArgs(remind.Email, "Period Course Change Remind", mailContent, remind.RemindId));
            }
            foreach (var mail in notifications)
            {
                _notificationObservable.send(mail);
            }

            result.Data = amendment;

            return Ok(result);
        }
    }
}
