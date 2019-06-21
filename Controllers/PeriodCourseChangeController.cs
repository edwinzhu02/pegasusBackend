using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodCourseChangeController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;
        private readonly IConfiguration _configuration;

        public PeriodCourseChangeController(pegasusContext.ablemusicContext ablemusicContext, IConfiguration configuration)
        {
            _ablemusicContext = ablemusicContext;
            _configuration = configuration;
        }

        // POST: api/PeriodCourseChange
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostPeriodCourseChange([FromBody] PeriodCourseChangeViewModel inputObj)
        {
            var result = new Result<List<Amendment>>();
            List<Lesson> exsitingLessons;
            Learner learner;
            List<Holiday> holidays;
            List<Amendment> exsitingAmendments;
            Amendment amendment = new Amendment();
            DateTime? todoDateEnd = null;
            DateTime todoDateBegin = inputObj.BeginDate;
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
            TodoList learnerTodoEndTime = new TodoList();
            TodoList teacherTodoEndTime = new TodoList();
            if (inputObj.EndDate.HasValue)
            {
                learnerTodoEndTime = TodoListForLearnerCreater(courseInfo, newCourseInfo, inputObj, todoDateEnd.Value);
                teacherTodoEndTime = TodoListForTeacherCreater(courseInfo, newCourseInfo, inputObj, todoDateEnd.Value);
            }
            TodoList learnerTodoBeginTime = TodoListForLearnerCreater(courseInfo, newCourseInfo, inputObj, todoDateBegin);
            TodoList teacherTodoBeginTime = TodoListForTeacherCreater(courseInfo, newCourseInfo, inputObj, todoDateBegin);
            RemindLog learnerRemindLog = RemindLogForLearnerCreater(courseInfo, newCourseInfo, inputObj);
            RemindLog teacherRemindLog = RemindLogForTeacherCreater(courseInfo, newCourseInfo, inputObj);

            try
            {
                await _ablemusicContext.Amendment.AddAsync(amendment);
                if (inputObj.IsTemporary == 1)
                {
                    await _ablemusicContext.TodoList.AddAsync(learnerTodoEndTime);
                    await _ablemusicContext.TodoList.AddAsync(teacherTodoEndTime);
                }
                await _ablemusicContext.TodoList.AddAsync(learnerTodoBeginTime);
                await _ablemusicContext.TodoList.AddAsync(teacherTodoBeginTime);

                await _ablemusicContext.RemindLog.AddAsync(learnerRemindLog);
                await _ablemusicContext.RemindLog.AddAsync(teacherRemindLog);

                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            //sending Email
            string mailTitle = "Period Course Change Remind";
            string teacherName = courseInfo.TeacherFirstName + " " + courseInfo.TeacherLastName;
            string contentForTeacher = MailContentGenerator(teacherName, courseInfo, newCourseInfo, inputObj);
            Task teacherMailSenderTask = MailSenderService.SendMailAsync(courseInfo.TeacherEmail, mailTitle, contentForTeacher, teacherRemindLog.RemindId);

            string learnerName = courseInfo.LearnerFirstName + " " + courseInfo.LearnerLastName;
            string mailContentForLearner = MailContentGenerator(learnerName, courseInfo, newCourseInfo, inputObj);
            Task learnerMailSenderTask = MailSenderService.SendMailAsync(courseInfo.LearnerEmail, mailTitle, mailContentForLearner, learnerRemindLog.RemindId);

            return Ok(result);
        }

        private TodoList TodoListForLearnerCreater(dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj, DateTime todoDate)
        {
            TodoList todolistForLearner = new TodoList();
            todolistForLearner.ListName = "Period Course Change Remind";
            todolistForLearner.ListContent = "Inform learner " + courseInfo.LearnerFirstName + " " + courseInfo.LearnerLastName +
                " the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " + 
                getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + getDayOfWeek(inputObj.DayOfWeek) + " ";
            todolistForLearner.ListContent += inputObj.IsTemporary == 1 ? "for the period between " + inputObj.BeginDate + " to " +
                inputObj.EndDate + "Temporarily" : "from " + inputObj.BeginDate + "permanently";
            todolistForLearner.CreatedAt = toNZTimezone(DateTime.UtcNow);
            todolistForLearner.ProcessedAt = null;
            todolistForLearner.ProcessFlag = 0;
            todolistForLearner.UserId = inputObj.UserId;
            todolistForLearner.TodoDate = todoDate;
            todolistForLearner.LearnerId = inputObj.LearnerId;
            todolistForLearner.LessonId = null;
            todolistForLearner.TeacherId = null;
            return todolistForLearner;
        }

        private TodoList TodoListForTeacherCreater(dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj, DateTime todoDate)
        {
            TodoList todolistForTeacher = new TodoList();
            todolistForTeacher.ListName = "Period Course Change Remind";
            todolistForTeacher.ListContent = "Inform teacher " + courseInfo.TeacherFirstName + " " + courseInfo.TeacherLastName +
                " the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + getDayOfWeek(inputObj.DayOfWeek) + " ";
            todolistForTeacher.ListContent += inputObj.IsTemporary == 1 ? "for the period between " + inputObj.BeginDate + " to " +
                inputObj.EndDate + "Temporarily" : "from " + inputObj.BeginDate + "permanently";
            todolistForTeacher.CreatedAt = toNZTimezone(DateTime.UtcNow);
            todolistForTeacher.ProcessedAt = null;
            todolistForTeacher.ProcessFlag = 0;
            todolistForTeacher.UserId = inputObj.UserId;
            todolistForTeacher.TodoDate = todoDate;
            todolistForTeacher.LearnerId = null;
            todolistForTeacher.LessonId = null;
            todolistForTeacher.TeacherId = courseInfo.TeacherId;
            return todolistForTeacher;
        }

        private RemindLog RemindLogForLearnerCreater(dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj)
        {
            RemindLog remindLogLearner = new RemindLog();
            remindLogLearner.LearnerId = courseInfo.LearnerId;
            remindLogLearner.Email = courseInfo.LearnerEmail;
            remindLogLearner.RemindType = 1;
            remindLogLearner.RemindContent = "Inform learner " + courseInfo.LearnerFirstName + " " + courseInfo.LearnerLastName +
                " the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + getDayOfWeek(inputObj.DayOfWeek) + " ";
            remindLogLearner.RemindContent += inputObj.IsTemporary == 1 ? "for the period between " + inputObj.BeginDate + " to " +
                inputObj.EndDate + "Temporarily" : "from " + inputObj.BeginDate + "permanently";
            remindLogLearner.CreatedAt = toNZTimezone(DateTime.UtcNow);
            remindLogLearner.TeacherId = null;
            remindLogLearner.IsLearner = 1;
            remindLogLearner.ProcessFlag = 0;
            remindLogLearner.EmailAt = null;
            remindLogLearner.RemindTitle = "Period Course Change Remind";
            remindLogLearner.ReceivedFlag = 0;
            remindLogLearner.LessonId = null;
            return remindLogLearner;
        }

        private RemindLog RemindLogForTeacherCreater(dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj)
        {
            RemindLog remindLogForTeacher = new RemindLog();
            remindLogForTeacher.LearnerId = null;
            remindLogForTeacher.Email = courseInfo.TeacherEmail;
            remindLogForTeacher.RemindType = 1;
            remindLogForTeacher.RemindContent = "Inform teacher " + courseInfo.TeacherFirstName + " " + courseInfo.TeacherLastName +
                " the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + getDayOfWeek(inputObj.DayOfWeek) + " ";
            remindLogForTeacher.RemindContent += inputObj.IsTemporary == 1 ? "for the period between " + inputObj.BeginDate + " to " +
                inputObj.EndDate + "Temporarily" : "from " + inputObj.BeginDate + "permanently";
            remindLogForTeacher.CreatedAt = toNZTimezone(DateTime.UtcNow);
            remindLogForTeacher.TeacherId = courseInfo.TeacherId;
            remindLogForTeacher.IsLearner = 0;
            remindLogForTeacher.ProcessFlag = 0;
            remindLogForTeacher.EmailAt = null;
            remindLogForTeacher.RemindTitle = "Period Course Change Remind";
            remindLogForTeacher.ReceivedFlag = 0;
            remindLogForTeacher.LessonId = null;
            return remindLogForTeacher;
        }

        private string MailContentGenerator(string name, dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>This is to inform you that the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + getDayOfWeek(inputObj.DayOfWeek) + " ";
            mailContent += inputObj.IsTemporary == 1 ? "for the period between " + inputObj.BeginDate + " to " +
                inputObj.EndDate + "Temporarily" : "from " + inputObj.BeginDate + "permanently</p>";

            return mailContent;
        }

        private string getDayOfWeek(int value)
        {
            switch (value)
            {
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
                case 7: return "Sunday";
                default: return ("Day of week not found");
            }
        }
    }
}
