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
        //splite a lesson 
        [HttpPut("{lessonId}/{userId}/{reason}")]
        public async Task<IActionResult> Put(int lessonId, short userId, string reason)
        {
            var result = new Result<List<object>>();
            Lesson lesson;
            Invoice invoice;
            Learner learner;
            List<Course> courses;
            List<Lesson> remainLessons;
            List<Holiday> holidays;
            LessonRemain lessonRemain;
            try
            {
                lesson = await _ablemusicContext.Lesson.Where(l => l.LessonId == lessonId).FirstOrDefaultAsync();
                if(lesson == null)
                {
                    throw new Exception("Lesson not found");
                }
                invoice = await _ablemusicContext.Invoice.Where(iv => iv.InvoiceNum == lesson.InvoiceNum && iv.IsActive == 1).FirstOrDefaultAsync();
                if(invoice == null)
                {
                    throw new Exception("This session may is a group session or Trial session, this session is not allowed to reschedule");
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
                                           InvoiceNum = l.InvoiceNum,
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
                                           Org = null,
                                           Room = null,
                                       }).ToListAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == lesson.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
                lessonRemain = await _ablemusicContext.LessonRemain.Where(lr => lr.TermId == invoice.TermId &&
                lr.CourseInstanceId == lesson.CourseInstanceId && lr.LearnerId == lesson.LearnerId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.Message;
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
                result.ErrorMessage = "Not enough remain lessons to do reschedule\n" + "Remain lessons found:" + 
                    remainLessons.Count + "\nRemain lessons need:" + numOfSchedulesToBeAdd;
                result.Data = new List<object>();
                foreach(var rl in remainLessons)
                {
                    result.Data.Add(new { RemainLesson = rl });
                }
                return BadRequest(result);
            }
            string courseName = courses[0].CourseName;

            DateTime min = remainLessons[0].BeginTime.Value;
            DateTime max = remainLessons[0].BeginTime.Value;
            foreach (var remainLesson in remainLessons)
            {
                min = remainLesson.BeginTime.Value > min ? min : remainLesson.BeginTime.Value;
                max = remainLesson.BeginTime.Value > max ? remainLesson.BeginTime.Value : max;
            }
            List<Lesson> lessonsToBeAppend = new List<Lesson>();
            int i = numOfSchedulesToBeAdd;
            var lessonConflictCheckerService = new LessonConflictCheckerService(min, max);
            try
            {
                await lessonConflictCheckerService.LoadAllProtentialConflictLessonsToMemoryAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            Dictionary<int, object> lessonIdMapConflictCheckResult = new Dictionary<int, object>();
            foreach (var remainLesson in remainLessons)
            {
                if (i <= 0) break;
                remainLesson.EndTime = remainLesson.EndTime.Value.AddMinutes(15);
                lessonConflictCheckerService.ConfigureLessonToCheck(remainLesson);
                Result<List<object>> lessonConflictCheckResult = lessonConflictCheckerService.CheckBothRoomAndTeacherInMemory();
                Result<List<object>> lessonConflictRecheckResult = new Result<List<object>>();
                if (lessonConflictCheckResult.IsSuccess)
                {
                    lessonsToBeAppend.Add(remainLesson);
                    i--;
                } else
                {
                    remainLesson.EndTime = remainLesson.EndTime.Value.AddMinutes(-15);
                    remainLesson.BeginTime = remainLesson.BeginTime.Value.AddMinutes(-15);
                    lessonConflictCheckerService.ConfigureLessonToCheck(remainLesson);
                    lessonConflictRecheckResult = lessonConflictCheckerService.CheckBothRoomAndTeacherInMemory();
                    if (lessonConflictRecheckResult.IsSuccess)
                    {
                        lessonsToBeAppend.Add(remainLesson);
                        i--;
                    }
                    else
                    {
                        remainLesson.BeginTime = remainLesson.BeginTime.Value.AddMinutes(15);
                        lessonIdMapConflictCheckResult.Add(remainLesson.LessonId, new
                        {
                            TimeBeforeConflict = lessonConflictRecheckResult,
                            TimeAfterConflict = lessonConflictCheckResult
                        });
                    }
                }
            }

            if (lessonsToBeAppend.Count < numOfSchedulesToBeAdd)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Not enough valid remain lessons to do reschedule.\n" + 
                    "Valid lessons found: " + lessonsToBeAppend.Count +
                    "\nRemain Lessons Found:" + remainLessons.Count + 
                    "\nRemain Lessons with Conflict:" + (remainLessons.Count - lessonsToBeAppend.Count) +
                    "\n Number of valid lessons need: " + numOfSchedulesToBeAdd;
                result.Data = new List<object>();
                foreach(var l in lessonsToBeAppend)
                {
                    result.Data.Add(new {ValidLesson = l});
                }
                foreach(var map in lessonIdMapConflictCheckResult)
                {
                    result.Data.Add(new
                    {
                        RemainLessonIdWithConflict = map.Key,
                        ConflictDetail = map.Value
                    });
                }
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
            DateTime remindScheduledDate = todoDate;

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

            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                todoRepository.AddSingleTodoList("Lesson Reschedule Remind", TodoListContentGenerator.LessonRescheduleForLearner(lesson, learner,
                    lessonsToBeAppend, courseName), userId, todoDate, lesson.LessonId, learner.LearnerId, null);
                todoRepository.AddMutipleTodoLists("Lesson Reschedule Remind", teacherIdMapTodoContent, userId, todoDate, lesson.LessonId, null);
                var saveTodoResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoResult.IsSuccess)
                {
                    return BadRequest(saveTodoResult);
                }

                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                remindLogRepository.AddSingleRemindLog(learner.LearnerId, learner.Email, RemindLogContentGenerator.LessonRescheduleForLearner(lesson, learner,
                    lessonsToBeAppend, courseName), null, "Lesson Reschedule Remind", lesson.LessonId, remindScheduledDate);
                remindLogRepository.AddMultipleRemindLogs(teacherMapRemindLogContent, null, "Lesson Reschedule Remind", lesson.LessonId, remindScheduledDate);
                var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindLogResult.IsSuccess)
                {
                    return BadRequest(saveRemindLogResult);
                }

                result.Data = new List<object>();
                foreach (var lessonAppend in lessonsToBeAppend)
                {
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
                        IsPaid = lessonAppend.IsPaid,
                        IsChanged = lessonAppend.IsChanged,
                        IsConfirm = lessonAppend.IsConfirm,
                        BeginTime = lessonAppend.BeginTime,
                        EndTime = lessonAppend.EndTime,
                        InvoiceNum = lessonAppend.InvoiceNum,
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
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }

                string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;
                //sending Email
                //List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
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
                    remindLogRepository.UpdateContent(remind.RemindId, mailContent);
                    //notifications.Add(new NotificationEventArgs(remind.Email, "Lesson Reschedule Confirm", mailContent, remind.RemindId));
                }
                //foreach (var mail in notifications)
                //{
                //    _notificationObservable.send(mail);
                //}
                var remindLogUpdateContentResult = await remindLogRepository.SaveUpdatedContentAsync();
                if (!remindLogUpdateContentResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = remindLogUpdateContentResult.ErrorMessage;
                    return BadRequest(result);
                }
                dbContextTransaction.Commit();
            }
            return Ok(result);
        }
    }
}
