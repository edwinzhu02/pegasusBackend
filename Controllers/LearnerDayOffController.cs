using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Repositories;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerDayOffController : BasicController
    {
        private readonly IConfiguration _configuration;

        public LearnerDayOffController(ablemusicContext ablemusicContext, ILogger<LearnerDayOffController> log, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _configuration = configuration;
        }

        // POST: api/LearnerDayOff
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostDayoff([FromBody] LearnerDayoffViewModel inputObj)
        {
            var result = new Result<List<Amendment>>();
            List<Lesson> lessons;
            List<Amendment> amendments = new List<Amendment>();
            List<Amendment> exsitsAmendment;
            Learner learner;
            dynamic courseSchedules;
            List<Holiday> holidays;
            try
            {
                lessons = await _ablemusicContext.Lesson.Where(l => l.LearnerId == inputObj.LearnerId && 
                l.BeginTime.Value.Date > inputObj.BeginDate.Date && l.BeginTime.Value.Date < inputObj.EndDate.Date).ToListAsync();
                courseSchedules = await (from i in _ablemusicContext.One2oneCourseInstance
                                         join cs in _ablemusicContext.CourseSchedule on i.CourseInstanceId equals cs.CourseInstanceId
                                         join l in _ablemusicContext.Learner on i.LearnerId equals l.LearnerId
                                         join t in _ablemusicContext.Teacher on i.TeacherId equals t.TeacherId
                                         join c in _ablemusicContext.Course on i.CourseId equals c.CourseId
                                         join o in _ablemusicContext.Org on i.OrgId equals o.OrgId
                                         join r in _ablemusicContext.Room on i.RoomId equals r.RoomId
                                         where inputObj.InstanceIds.Contains(cs.CourseInstanceId.Value)
                                         select new
                                         {
                                             CourseScheduleId = cs.CourseScheduleId,
                                             CourseInstanceId = cs.CourseInstanceId,
                                             OrgId = i.OrgId,
                                             OrgName = o.OrgName,
                                             RoomId = i.RoomId,
                                             RoomName = r.RoomName,
                                             CourseId = i.CourseId,
                                             CourseName = c.CourseName,
                                             TeacherId = i.TeacherId,
                                             TeacherFirstName = t.FirstName,
                                             TeacherLastName = t.LastName,
                                             TeacherEmail = t.Email,
                                             DayOfWeek = cs.DayOfWeek,
                                             LearnerId = i.LearnerId,
                                             LearnerFirstName = l.FirstName,
                                             LearnerLastName = l.LastName,
                                             LearnerEmail = l.Email,
                                         }).ToListAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == inputObj.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
                exsitsAmendment = await _ablemusicContext.Amendment.Where(a => a.LearnerId == inputObj.LearnerId && a.AmendType == 1 &&
                ((inputObj.BeginDate >= a.BeginDate && inputObj.BeginDate <= a.EndDate) ||
                (inputObj.EndDate >= a.BeginDate && inputObj.EndDate <= a.EndDate) ||
                (inputObj.BeginDate <= a.BeginDate && inputObj.EndDate >= a.EndDate))).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if(courseSchedules.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Course schedule not found";
                return BadRequest(result);
            }
            if(learner == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Learner not found";
                return BadRequest(result);
            }
            if(exsitsAmendment.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "There is a conflict of date on your previous dayoff";
                return BadRequest(result);
            }

            foreach(var lesson in lessons)
            {
                lesson.IsCanceled = 1;
                lesson.Reason = inputObj.Reason;
            }

            foreach(var cs in courseSchedules)
            {
                if(cs.LearnerId != inputObj.LearnerId)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "InstanceId not match learnerId";
                    return BadRequest(result);
                }
                Amendment amendment = new Amendment();
                amendment.CourseInstanceId = cs.CourseInstanceId;
                amendment.OrgId = cs.OrgId;
                amendment.DayOfWeek = cs.DayOfWeek;
                amendment.BeginTime = null;
                amendment.EndTime = null;
                amendment.LearnerId = cs.LearnerId;
                amendment.RoomId = cs.RoomId;
                amendment.BeginDate = inputObj.BeginDate;
                amendment.EndDate = inputObj.EndDate;
                amendment.CreatedAt = toNZTimezone(DateTime.UtcNow);
                amendment.Reason = inputObj.Reason;
                amendment.IsTemporary = null;
                amendment.AmendType = 1;
                amendment.CourseScheduleId = cs.CourseScheduleId;
                amendments.Add(amendment);
            }

            DateTime todoDate = inputObj.EndDate;
            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }
            }

            var teacherIdMapTodoContent = new Dictionary<short, string>();
            var teacherMapRemindLogContent = new Dictionary<Teacher, string>();

            foreach (var cs in courseSchedules)
            {
                teacherIdMapTodoContent.Add(cs.TeacherId, TodoListContentGenerator.DayOffForTeacher(cs, inputObj.EndDate.ToString()));
                teacherMapRemindLogContent.Add(new Teacher
                {
                    TeacherId = cs.TeacherId,
                    Email = cs.TeacherEmail
                }, RemindLogContentGenerator.DayOffForTeacher(cs, inputObj.EndDate.ToString()));
            }

            TodoRepository todoRepository = new TodoRepository();
            todoRepository.AddSingleTodoList("Period Dayoff Remind", TodoListContentGenerator.DayOffForLearner(courseSchedules[0],
                inputObj.EndDate.ToString()), inputObj.UserId, todoDate, null, courseSchedules[0].LearnerId, null);
            todoRepository.AddMutipleTodoLists("Period Dayoff Remind", teacherIdMapTodoContent, inputObj.UserId, todoDate, null, null);
            var saveTodoResult = await todoRepository.SaveTodoListsAsync();
            if (!saveTodoResult.IsSuccess)
            {
                return BadRequest(saveTodoResult);
            }

            RemindLogRepository remindLogRepository = new RemindLogRepository();
            remindLogRepository.AddSingleRemindLog(courseSchedules[0].LearnerId, courseSchedules[0].LearnerEmail,
                RemindLogContentGenerator.DayOffForLearner(courseSchedules[0], inputObj.EndDate.ToString()), null, "Period Dayoff Remind", null);
            remindLogRepository.AddMultipleRemindLogs(teacherMapRemindLogContent, null, "Period Dayoff Remind", null);
            var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
            if (!saveRemindLogResult.IsSuccess)
            {
                return BadRequest(saveRemindLogResult);
            }

            try
            {
                foreach (var amendment in amendments)
                {
                    await _ablemusicContext.Amendment.AddAsync(amendment);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;
            //sending Email
            List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
            foreach (var todo in saveTodoResult.Data)
            {
                var remind = saveRemindLogResult.Data.Find(r => r.LearnerId == todo.LearnerId && r.TeacherId == todo.TeacherId);
                string currentPersonName = "";
                dynamic currentCourseSchedule = null;
                if (todo.TeacherId == null)
                {
                    foreach(var cs in courseSchedules)
                    {
                        if (todo.LearnerId == cs.LearnerId)
                        {
                            currentPersonName = cs.LearnerFirstName + " " + cs.LearnerLastName;
                            currentCourseSchedule = cs;
                        }
                    }
                }
                else
                {
                    foreach(var cs in courseSchedules)
                    {
                        if (todo.TeacherId == cs.TeacherId)
                        {
                            currentPersonName = cs.TeacherFirstName + " " + cs.TeacherLastName;
                            currentCourseSchedule = cs;
                        }
                    }
                }
                string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                string mailContent = EmailContentGenerator.Dayoff(currentPersonName, currentCourseSchedule, inputObj, confirmURL);
                notifications.Add(new NotificationEventArgs(remind.Email, "Dayoff is expired", mailContent, remind.RemindId));
            }
            foreach (var mail in notifications)
            {
                _notificationObservable.send(mail);
            }

            foreach(var amendment in amendments)
            {
                amendment.Learner = null;
            }
            result.Data = amendments;
            return Ok(result);
        }
    }
}
