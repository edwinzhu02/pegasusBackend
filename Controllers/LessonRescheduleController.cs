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
using Pegasus_backend.Utilities;
using Pegasus_backend.Repositories;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonRescheduleController : BasicController
    {
        private readonly IConfiguration _configuration;

        public LessonRescheduleController(ablemusicContext ablemusicContext, ILogger<LessonRescheduleController> log, IConfiguration configuration) : base(ablemusicContext, log)
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
            LessonRemain lessonRemain;
            try
            {
                lesson = await _ablemusicContext.Lesson.Where(l => l.LessonId == lessonId).Include(l => l.Invoice).FirstOrDefaultAsync();
                if(lesson.Invoice == null)
                {
                    throw new Exception("Invoice not found under the give lesson id");
                }
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
                lessonRemain = await _ablemusicContext.LessonRemain.Where(lr => lr.TermId == lesson.Invoice.TermId &&
                lr.CourseInstanceId == lesson.CourseInstanceId && lr.LearnerId == lesson.LearnerId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.ToString();
                return NotFound(result);
            }
            if (lesson == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "lesson id not found";
                return NotFound(result);
            }
            if (lesson.GroupCourseInstanceId != null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Group course is not allowed to reschedule";
                return BadRequest(result);
            }
            if (lesson.IsCanceled == 1)
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
            List<Lesson> lessonsToBeAppend = new List<Lesson>();
            int i = numOfSchedulesToBeAdd;
            foreach (var remainLesson in remainLessons)
            {
                if (i <= 0) break;
                var lessonConflictCheckerService = new LessonConflictCheckerService(remainLesson);
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
                if (lessonConflictCheckResult.IsSuccess)
                {
                    lessonsToBeAppend.Add(remainLesson);
                    i--;
                }
            }

            if (lessonsToBeAppend.Count < numOfSchedulesToBeAdd)
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
                if (effectedTeachers.Find(et => et.TeacherId == lessonAppend.Teacher.TeacherId) == null)
                {
                    effectedTeachers.Add(lessonAppend.Teacher);
                }
            }

            var teacherIdMapTodoContent = new Dictionary<short, string>();
            var teacherMapRemindLogContent = new Dictionary<Teacher, string>();
            foreach(var teacher in effectedTeachers)
            {
                teacherIdMapTodoContent.Add(teacher.TeacherId, TodoListContentGenerator.LessonRescheduleForTeacher(lesson, lessonsToBeAppend, teacher, courseName));
                teacherMapRemindLogContent.Add(teacher, RemindLogContentGenerator.LessonRescheduleForTeacher(lesson, lessonsToBeAppend, teacher, courseName));
            }

            TodoRepository todoRepository = new TodoRepository();
            todoRepository.AddSingleTodoList("Lesson Reschedule Remind", TodoListContentGenerator.LessonRescheduleForLearner(lesson, learner,
                lessonsToBeAppend, courseName), userId, todoDate, lesson.LessonId, learner.LearnerId, null);
            todoRepository.AddMutipleTodoLists("Lesson Reschedule Remind", teacherIdMapTodoContent, userId, todoDate, lesson.LessonId, null);
            var saveTodoResult = await todoRepository.SaveTodoListsAsync();
            if (!saveTodoResult.IsSuccess)
            {
                return BadRequest(saveTodoResult);
            }

            RemindLogRepository remindLogRepository = new RemindLogRepository();
            remindLogRepository.AddSingleRemindLog(learner.LearnerId, learner.Email, RemindLogContentGenerator.LessonRescheduleForLearner(lesson, learner,
                lessonsToBeAppend, courseName), null, "Lesson Reschedule Remind", lesson.LessonId);
            remindLogRepository.AddMultipleRemindLogs(teacherMapRemindLogContent, null, "Lesson Reschedule Remind", lesson.LessonId);
            var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
            if (!saveRemindLogResult.IsSuccess)
            {
                return BadRequest(saveRemindLogResult);
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
            lessonRemain.Quantity -= 1;

            try
            {
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
                string currentPersonName;
                if (todo.TeacherId == null)
                {
                    currentPersonName = learner.FirstName + " " + learner.LastName;
                }
                else
                {
                    var currentTeacher = effectedTeachers.Find(t => t.TeacherId == todo.TeacherId);
                    currentPersonName = currentTeacher.FirstName + " " + currentTeacher.LastName;
                }
                string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                string mailContent = EmailContentGenerator.LessonReschedule(currentPersonName, courseName, lesson, reason, confirmURL, lessonsToBeAppend);
                notifications.Add(new NotificationEventArgs(remind.Email, "Lesson Reschedule Confirm", mailContent, remind.RemindId));
            }
            foreach (var mail in notifications)
            {
                _notificationObservable.send(mail);
            }

            return Ok(result);
        }
    }
}
