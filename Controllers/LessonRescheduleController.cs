using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonRescheduleController : BasicController
    {
        private readonly IConfiguration _configuration;

        public LessonRescheduleController(ablemusicContext ablemusicContext, ILogger<ValuesController> log, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _configuration = configuration;
        }

        // PUT: api/LessonReschedule/5
        [HttpPut("{lessonId}/{userId}/{reason}")]
        public async Task<IActionResult> Put(int lessonId, short userId, string reason)
        {
            var result = new Result<List<Lesson>>();
            Lesson lesson;
            Learner learner;
            List<Course> courses;
            List<Lesson> remainLessons;
            List<Holiday> holidays;
            try
            {
                lesson = await _ablemusicContext.Lesson.Where(l => l.LessonId == lessonId).FirstOrDefaultAsync();
                courses = await (from c in _ablemusicContext.Course
                                 join oto in _ablemusicContext.One2oneCourseInstance on c.CourseId equals oto.CourseId
                                 where oto.CourseInstanceId == lesson.CourseInstanceId
                                 select new Course
                                 {
                                     CourseId = c.CourseId,
                                     CourseName = c.CourseName,
                                     Duration = c.Duration,
                                 }).ToListAsync();
                remainLessons = await (from l in _ablemusicContext.Lesson
                                       where l.LearnerId == lesson.LearnerId && l.CourseInstanceId == lesson.CourseInstanceId &&
                                       l.BeginTime > lesson.BeginTime && l.IsCanceled != 1
                                       select new Lesson
                                       {
                                           LessonId = l.LessonId,
                                           LearnerId = l.LearnerId,
                                           RoomId = l.RoomId,
                                           TeacherId = l.TeacherId,
                                           OrgId = l.OrgId,
                                           IsCanceled = l.IsCanceled,
                                           Reason = l.Reason,
                                           CreatedAt = l.CreatedAt,
                                           CourseInstanceId = l.CourseInstanceId,
                                           GroupCourseInstanceId = l.GroupCourseInstanceId,
                                           IsTrial = l.IsTrial,
                                           BeginTime = l.BeginTime,
                                           EndTime = l.EndTime,
                                           InvoiceId = l.InvoiceId,
                                           Learner = null,
                                           Teacher = new Teacher
                                           {
                                               TeacherId = l.Teacher.TeacherId,
                                               FirstName = l.Teacher.FirstName,
                                               LastName = l.Teacher.LastName,
                                               Email = l.Teacher.Email,
                                               User = null,
                                               AvailableDays = null,
                                               GroupCourseInstance = null,
                                               Lesson = null,
                                               One2oneCourseInstance = null,

                                           },
                                           CourseInstance = null,
                                           GroupCourseInstance = null,
                                           Invoice = null,
                                           Org = null,
                                           Room = null,
                                       }).ToListAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == lesson.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.ToString();
                return NotFound(result);
            }
            if(lesson == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "lesson id not found";
                return NotFound(result);
            }
            if(lesson.GroupCourseInstanceId != null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Group course is not allowed to reschedule";
                return BadRequest(result);
            }
            if(lesson.IsCanceled == 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "This lesson has been already canelled";
                return BadRequest(result);
            }
            if (courses.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Fail to found course name under this lesson";
                return NotFound(result);
            }
            int numOfSchedulesToBeAdd = courses[0].Duration.Value + 1;
            if (remainLessons.Count < numOfSchedulesToBeAdd)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Not enough remain lessons to do reschedule";
                return BadRequest(result);
            }
            string courseName = courses[0].CourseName;
            List<Lesson> conflictRemainLessons = new List<Lesson>();
            List<Lesson> lessonsToBeAppend = new List<Lesson>();
            int i = numOfSchedulesToBeAdd;
            foreach (var remainLesson in remainLessons)
            {
                if (i <= 0) break;
                List<Lesson> conflictRooms = new List<Lesson>();
                List<Lesson> conflictTeacherLessons = new List<Lesson>();
                try
                {
                    conflictRooms = await _ablemusicContext.Lesson.Where(l => l.RoomId == remainLesson.RoomId &&
                        l.OrgId == remainLesson.OrgId && remainLesson.LessonId != l.LessonId && l.IsCanceled != 1 &&
                        ((l.BeginTime > remainLesson.BeginTime && l.BeginTime < remainLesson.EndTime) ||
                        (l.EndTime > remainLesson.BeginTime && l.EndTime < remainLesson.EndTime) ||
                        (l.BeginTime <= remainLesson.BeginTime && l.EndTime >= remainLesson.EndTime)))
                        .ToListAsync();

                    DateTime beginTime = remainLesson.BeginTime.Value.AddMinutes(-60);
                    DateTime endTime = remainLesson.EndTime.Value.AddMinutes(75);
                    conflictTeacherLessons = await _ablemusicContext.Lesson.Where(l => l.TeacherId == remainLesson.TeacherId &&
                    l.LessonId != remainLesson.LessonId &&
                    ((l.BeginTime > beginTime && l.BeginTime < endTime) ||
                    (l.EndTime > beginTime && l.EndTime < endTime) ||
                    (l.BeginTime <= beginTime && l.EndTime >= endTime)))
                    .ToListAsync();

                    if (conflictRooms.Count > 0)
                    {
                        conflictRemainLessons.Add(remainLesson);
                    }
                    else
                    {
                        if (conflictTeacherLessons.Count > 0)
                        {
                            bool teacherHasConflict = false;
                            foreach (var c in conflictTeacherLessons)
                            {
                                if (c.OrgId != remainLesson.OrgId ||
                                    (c.BeginTime > remainLesson.BeginTime && c.BeginTime < remainLesson.EndTime) ||
                                    (c.EndTime > remainLesson.BeginTime && c.EndTime < remainLesson.EndTime) ||
                                    (c.BeginTime <= remainLesson.BeginTime && c.EndTime >= remainLesson.EndTime))
                                {
                                    teacherHasConflict = true;
                                    break;
                                }
                            }
                            if (teacherHasConflict)
                            {
                                conflictRemainLessons.Add(remainLesson);
                            }
                            else
                            {
                                lessonsToBeAppend.Add(remainLesson);
                                i--;
                            }
                        }
                        else
                        {
                            lessonsToBeAppend.Add(remainLesson);
                            i--;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.ToString();
                    return BadRequest(result);
                }
            }

            if(lessonsToBeAppend.Count < numOfSchedulesToBeAdd)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Not enough valid remain lessons to do reschedule";
                return BadRequest(result);
            }
           
            DateTime todoDate = lesson.BeginTime.Value;
            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }
            }

            List<Teacher> effectedTeachers = new List<Teacher>();
            foreach (var lessonAppend in lessonsToBeAppend)
            {
                if (!effectedTeachers.Contains(lessonAppend.Teacher))
                {
                    effectedTeachers.Add(lessonAppend.Teacher);
                }
            }

            TodoList todoForLearner = TodoListForLearnerCreater(lesson, userId, learner, todoDate, lessonsToBeAppend, courseName);
            List<TodoList> todoForTeachers = new List<TodoList>();
            foreach(var teacher in effectedTeachers)
            {
                todoForTeachers.Add(TodoListForTeacherCreater(lesson, userId, learner, todoDate, lessonsToBeAppend, teacher, courseName));
            }

            RemindLog remindLogForLearner = RemindLogForLearnerCreater(lesson, learner, lessonsToBeAppend, courseName);
            List<RemindLog> remindLogsForTeachers = new List<RemindLog>();
            foreach(var teacher in effectedTeachers)
            {
                remindLogsForTeachers.Add(RemindLogForTeacherCreater(lesson, lessonsToBeAppend, teacher, courseName));
            }

            result.Data = new List<Lesson>();

            foreach (var lessonAppend in lessonsToBeAppend)
            {
                lessonAppend.EndTime = lessonAppend.EndTime.Value.AddMinutes(15);
                result.Data.Add(new Lesson
                {
                    LessonId = lessonAppend.LessonId,
                    LearnerId = lessonAppend.LearnerId,
                    RoomId = lessonAppend.RoomId,
                    TeacherId = lessonAppend.TeacherId,
                    OrgId = lessonAppend.OrgId,
                    IsCanceled = lessonAppend.IsCanceled,
                    Reason = lessonAppend.Reason,
                    CreatedAt = lessonAppend.CreatedAt,
                    CourseInstanceId = lessonAppend.CourseInstanceId,
                    GroupCourseInstanceId = lessonAppend.GroupCourseInstanceId,
                    IsTrial = lessonAppend.IsTrial,
                    BeginTime = lessonAppend.BeginTime,
                    EndTime = lessonAppend.EndTime,
                    InvoiceId = lessonAppend.InvoiceId,
                });
                _ablemusicContext.Update(lessonAppend);
            }

            lesson.IsCanceled = 1;
            lesson.Reason = reason;

            try
            {
                await _ablemusicContext.TodoList.AddAsync(todoForLearner);
                await _ablemusicContext.RemindLog.AddAsync(remindLogForLearner);
                foreach(var t in todoForTeachers)
                {
                    await _ablemusicContext.TodoList.AddAsync(t);
                }
                foreach(var r in remindLogsForTeachers)
                {
                    await _ablemusicContext.RemindLog.AddAsync(r);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;
            //sending Email
            string mailTitle = "Lesson Reschedule Confirm";
            List<Task> teacherMailSenderTasks = new List<Task>();
            for (int j = 0; j < remindLogsForTeachers.Count; j++)
            {
                string confirmURLForTeacher = userConfirmUrlPrefix + todoForTeachers[j].ListId + "/" + remindLogsForTeachers[j].RemindId;
                string mailContentForTeacher = MailContentGenerator(effectedTeachers[j].FirstName + " " + effectedTeachers[j].LastName, courseName, lesson, reason, confirmURLForTeacher, lessonsToBeAppend);
                teacherMailSenderTasks.Add(MailSenderService.SendMailAsync(remindLogsForTeachers[j].Email, mailTitle, mailContentForTeacher, remindLogsForTeachers[j].RemindId));
            }
            string confirmURLForLearner = userConfirmUrlPrefix + todoForLearner.ListId + "/" + remindLogForLearner.RemindId;
            string mailContentForLearner = MailContentGenerator(learner.FirstName + " " + learner.LastName, courseName, lesson, reason, confirmURLForLearner, lessonsToBeAppend);
            Task learnerMailSenderTask = MailSenderService.SendMailAsync(remindLogForLearner.Email, mailTitle, mailContentForLearner, remindLogForLearner.RemindId);

            return Ok(result);
        }

        private TodoList TodoListForLearnerCreater(Lesson lesson, short userId, Learner learner, DateTime todoDate, 
            List<Lesson> appendLessons, string courseName)
        {
            TodoList todolistForLearner = new TodoList();
            todolistForLearner.ListName = "Lesson Reschedule Remind";
            todolistForLearner.ListContent = "Inform learner " + learner.FirstName + " " + learner.LastName +
                " the " + courseName + " lession from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been rescheduled. Lesson remain hours are append to the following lessions: \n";
            foreach(var appendLesson in appendLessons)
            {
                todolistForLearner.ListContent += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            todolistForLearner.CreatedAt = toNZTimezone(DateTime.UtcNow);
            todolistForLearner.ProcessedAt = null;
            todolistForLearner.ProcessFlag = 0;
            todolistForLearner.UserId = userId;
            todolistForLearner.TodoDate = todoDate;
            todolistForLearner.LearnerId = learner.LearnerId;
            todolistForLearner.LessonId = lesson.LessonId;
            todolistForLearner.TeacherId = null;
            return todolistForLearner;
        }

        private TodoList TodoListForTeacherCreater(Lesson lesson, short userId, Learner learner, DateTime todoDate, 
            List<Lesson> appendLessons, Teacher teacher, string courseName)
        {
            TodoList todolistForTeacher = new TodoList();
            todolistForTeacher.ListName = "Lesson Reschedule Remind";
            todolistForTeacher.ListContent = "Inform teacher " + teacher.FirstName + " " + teacher.LastName +
                " the " + courseName + " lession from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been rescheduled. Lesson Remain hours are append to the following lessons: \n";
            foreach (var appendLesson in appendLessons)
            {
                todolistForTeacher.ListContent += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            todolistForTeacher.CreatedAt = toNZTimezone(DateTime.UtcNow);
            todolistForTeacher.ProcessedAt = null;
            todolistForTeacher.ProcessFlag = 0;
            todolistForTeacher.UserId = userId;
            todolistForTeacher.TodoDate = todoDate;
            todolistForTeacher.LearnerId = null;
            todolistForTeacher.LessonId = lesson.LessonId;
            todolistForTeacher.TeacherId = teacher.TeacherId;
            return todolistForTeacher;
        }

        private RemindLog RemindLogForLearnerCreater(Lesson lesson, Learner learner, List<Lesson> appendLessons, string courseName)
        {
            RemindLog remindLogLearner = new RemindLog();
            remindLogLearner.LearnerId = learner.LearnerId;
            remindLogLearner.Email = learner.Email;
            remindLogLearner.RemindType = 1;
            remindLogLearner.RemindContent = "Inform learner " + learner.FirstName + " " + learner.LastName +
                " the " + courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been rescheduled. Lesson remain hours are append to the following lessions: \n";
            foreach (var appendLesson in appendLessons)
            {
                remindLogLearner.RemindContent += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            remindLogLearner.CreatedAt = toNZTimezone(DateTime.UtcNow);
            remindLogLearner.TeacherId = null;
            remindLogLearner.IsLearner = 1;
            remindLogLearner.ProcessFlag = 0;
            remindLogLearner.EmailAt = null;
            remindLogLearner.RemindTitle = "Lesson Reschedule Remind";
            remindLogLearner.ReceivedFlag = 0;
            remindLogLearner.LessonId = lesson.LessonId;
            return remindLogLearner;
        }

        private RemindLog RemindLogForTeacherCreater(Lesson lesson, List<Lesson> appendLessons, Teacher teacher, string courseName)
        {
            RemindLog remindLogForTeacher = new RemindLog();
            remindLogForTeacher.LearnerId = null;
            remindLogForTeacher.Email = teacher.Email;
            remindLogForTeacher.RemindType = 1;
            remindLogForTeacher.RemindContent = "Inform teacher " + teacher.FirstName + " " + teacher.LastName +
                " the " + courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been rescheduled. Lesson Remain hours are append to the following lessons: \n";
            foreach (var appendLesson in appendLessons)
            {
                remindLogForTeacher.RemindContent += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            remindLogForTeacher.CreatedAt = toNZTimezone(DateTime.UtcNow);
            remindLogForTeacher.TeacherId = teacher.TeacherId;
            remindLogForTeacher.IsLearner = 0;
            remindLogForTeacher.ProcessFlag = 0;
            remindLogForTeacher.EmailAt = null;
            remindLogForTeacher.RemindTitle = "Lesson Reschedule Remind";
            remindLogForTeacher.ReceivedFlag = 0;
            remindLogForTeacher.LessonId = lesson.LessonId;
            return remindLogForTeacher;
        }
        private string MailContentGenerator(string name, string courseName, Lesson lesson, string reason, string confirmURL, List<Lesson> appendLessons)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>Your " +
                    courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                    " has been rescheduled due to " + reason + ". Lesson Remain hours are append to the following lessons: \n";
            foreach (var appendLesson in appendLessons)
            {
                mailContent += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            mailContent += "\nPlease click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }
    }
}
