using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerDayOffController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;
        private readonly IConfiguration _configuration;

        public LearnerDayOffController(pegasusContext.ablemusicContext ablemusicContext, IConfiguration configuration)
        {
            _ablemusicContext = ablemusicContext;
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
            if(lessons.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Lesson not found";
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
                amendment.CreatedAt = DateTime.Now;
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

            TodoList learnerTodo = TodoListForLearnerCreater(courseSchedules[0], inputObj, todoDate);
            List<TodoList> teacherTodos = TodoListForTeachersCreater(courseSchedules, inputObj, todoDate);
            RemindLog learnerRemindLog = RemindLogForLearnerCreater(courseSchedules[0], inputObj);
            List<RemindLog> teacherRemindLogs = RemindLogsForTeachersCreater(courseSchedules, inputObj);

            try
            {
                await _ablemusicContext.TodoList.AddAsync(learnerTodo);
                foreach (var teacherTodo in teacherTodos)
                {
                    await _ablemusicContext.TodoList.AddAsync(teacherTodo);
                }
                await _ablemusicContext.RemindLog.AddAsync(learnerRemindLog);
                foreach (var teacherRemind in teacherRemindLogs)
                {
                    await _ablemusicContext.RemindLog.AddAsync(teacherRemind);
                }
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
            string mailTitle = "Dayoff is expired";
            List<Task> teacherMailSenderTasks = new List<Task>();
            for (int i = 0; i < teacherRemindLogs.Count; i++)
            {
                string confirmURLForTeacher = userConfirmUrlPrefix + teacherRemindLogs[i].RemindId;
                string teacherName = courseSchedules[i].TeacherFirstName + " " + courseSchedules[i].TeacherLastName;
                string mailContentForTeacher = MailContentGenerator(teacherName, courseSchedules[i], inputObj, confirmURLForTeacher);
                teacherMailSenderTasks.Add(MailSenderService.SendMailAsync(teacherRemindLogs[i].Email, mailTitle, mailContentForTeacher, teacherRemindLogs[i].RemindId));
            }
            string confirmURLForLearner = userConfirmUrlPrefix + learnerRemindLog.RemindId;
            string learnerName = courseSchedules[0].LearnerFirstName + " " + courseSchedules[0].LearnerLastName;
            string mailContentForLearner = MailContentGenerator(learnerName, courseSchedules[0], inputObj, confirmURLForLearner);
            Task learnerMailSenderTask = MailSenderService.SendMailAsync(learnerRemindLog.Email, mailTitle, mailContentForLearner, learnerRemindLog.RemindId);

            foreach(var amendment in amendments)
            {
                amendment.Learner = null;
            }
            result.Data = amendments;
            return Ok(result);
        }

        private TodoList TodoListForLearnerCreater(dynamic courseSchedule, LearnerDayoffViewModel inputObj, DateTime todoDate)
        {
            TodoList todolistForLearner = new TodoList();
            todolistForLearner.ListName = "Period Dayoff Remind";
            todolistForLearner.ListContent = "Inform learner " + courseSchedule.LearnerFirstName + " " + courseSchedule.LearnerLastName +
                " the period of dayoff for the course: " + courseSchedule.CourseName + " will finish soon by " + inputObj.EndDate.ToString();
            todolistForLearner.CreatedAt = DateTime.Now;
            todolistForLearner.ProcessedAt = null;
            todolistForLearner.ProcessFlag = 0;
            todolistForLearner.UserId = inputObj.UserId;
            todolistForLearner.TodoDate = todoDate;
            todolistForLearner.LearnerId = courseSchedule.LearnerId;
            todolistForLearner.LessonId = null;
            todolistForLearner.TeacherId = null;
            return todolistForLearner;
        }

        private TodoList TodoListForTeacherCreater(dynamic courseSchedule, LearnerDayoffViewModel inputObj, DateTime todoDate)
        {
            TodoList todolistForTeacher = new TodoList();
            todolistForTeacher.ListName = "Period Dayoff Remind";
            todolistForTeacher.ListContent = "Inform teacher " + courseSchedule.TeacherFirstName + " " + courseSchedule.TeacherLastName +
                " the learner " + courseSchedule.LearnerFirstName + " " + courseSchedule.LearnerLastName + "'s dayoff for the course " + 
                courseSchedule.CourseName + " will finish soon by " + inputObj.EndDate.ToString();
            todolistForTeacher.CreatedAt = DateTime.Now;
            todolistForTeacher.ProcessedAt = null;
            todolistForTeacher.ProcessFlag = 0;
            todolistForTeacher.UserId = inputObj.UserId;
            todolistForTeacher.TodoDate = todoDate;
            todolistForTeacher.LearnerId = null;
            todolistForTeacher.LessonId = null;
            todolistForTeacher.TeacherId = courseSchedule.TeacherId;
            return todolistForTeacher;
        }

        private List<TodoList> TodoListForTeachersCreater(dynamic courseSchedules, LearnerDayoffViewModel inputObj, DateTime todoDate)
        {
            List<TodoList> todoListsForTeachers = new List<TodoList>();
            foreach(var courseSchedule in courseSchedules)
            {
                todoListsForTeachers.Add(TodoListForTeacherCreater(courseSchedule, inputObj, todoDate));
            }
            return todoListsForTeachers;
        }

        private RemindLog RemindLogForLearnerCreater(dynamic courseSchedule, LearnerDayoffViewModel inputObj)
        {
            RemindLog remindLogLearner = new RemindLog();
            remindLogLearner.LearnerId = courseSchedule.LearnerId;
            remindLogLearner.Email = courseSchedule.LearnerEmail;
            remindLogLearner.RemindType = 1;
            remindLogLearner.RemindContent = "Inform learner " + courseSchedule.LearnerFirstName + " " + courseSchedule.LearnerLastName +
                " the period of dayoff for the course: " + courseSchedule.CourseName + " will finish soon by " + inputObj.EndDate.ToString();
            remindLogLearner.CreatedAt = DateTime.Now;
            remindLogLearner.TeacherId = null;
            remindLogLearner.IsLearner = 1;
            remindLogLearner.ProcessFlag = 0;
            remindLogLearner.EmailAt = null;
            remindLogLearner.RemindTitle = "Period Dayoff Remind";
            remindLogLearner.ReceivedFlag = 0;
            remindLogLearner.LessonId = null;
            return remindLogLearner;
        }

        private RemindLog RemindLogForTeacherCreater(dynamic courseSchedule, LearnerDayoffViewModel inputObj)
        {
            RemindLog remindLogForTeacher = new RemindLog();
            remindLogForTeacher.LearnerId = null;
            remindLogForTeacher.Email = courseSchedule.TeacherEmail;
            remindLogForTeacher.RemindType = 1;
            remindLogForTeacher.RemindContent = "Inform teacher " + courseSchedule.TeacherFirstName + " " + courseSchedule.TeacherLastName + 
                " the learner " +courseSchedule.LearnerFirstName + " " + courseSchedule.LearnerLastName + "'s dayoff for the course " + 
                courseSchedule.CourseName + " will finish soon by " + inputObj.EndDate.ToString();
            remindLogForTeacher.CreatedAt = DateTime.Now;
            remindLogForTeacher.TeacherId = courseSchedule.TeacherId;
            remindLogForTeacher.IsLearner = 0;
            remindLogForTeacher.ProcessFlag = 0;
            remindLogForTeacher.EmailAt = null;
            remindLogForTeacher.RemindTitle = "Period Dayoff Remind";
            remindLogForTeacher.ReceivedFlag = 0;
            remindLogForTeacher.LessonId = null;
            return remindLogForTeacher;
        }

        private List<RemindLog> RemindLogsForTeachersCreater(dynamic courseSchedules, LearnerDayoffViewModel inputObj)
        {
            List<RemindLog> remindLogsForTeachers = new List<RemindLog>();
            foreach (var courseSchedule in courseSchedules)
            {
                remindLogsForTeachers.Add(RemindLogForTeacherCreater(courseSchedule, inputObj));
            }
            return remindLogsForTeachers;
        }

        private string MailContentGenerator(string name, dynamic courseSchedule, LearnerDayoffViewModel inputObj, string confirmURL)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>This is to inform you that the learner " + courseSchedule.LearnerFirstName +
                " " + courseSchedule.LearnerLastName + " has been taken the dayoff from " + inputObj.BeginDate.ToString() + " to " +
                inputObj.EndDate.ToString() + ". The course " + courseSchedule.CourseName + " in the period is canceled. </p>"; 

            mailContent += "<p> Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }
    }
}
