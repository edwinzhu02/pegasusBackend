﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;

        public SessionController(pegasusContext.pegasusContext pegasusContext, IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }

        // GET: api/Session
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetSession()
        {
            Result<List<Lesson>> result = new Result<List<Lesson>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.Lesson.Include(x => x.CourseInstance).Include(x => x.GroupCourseInstance).Include(x => x.Learner).Include(x => x.Org).Include(x => x.Room).Include(x => x.Teacher).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [Route("sessionCancelConfirm/{listId}/{remindId}")]
        [HttpGet]
        public async Task<IActionResult> CancelConfirm(int listId, int remindId)
        {
            var result = new Result<string>();
            var todoList = await _pegasusContext.TodoList.Where(t => t.ListId == listId).FirstOrDefaultAsync();
            var remindLog = await _pegasusContext.RemindLog.Where(r => r.RemindId == remindId).FirstOrDefaultAsync();
            if (todoList == null || remindLog == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = (todoList == null ? "list id is not found " : "") + (remindLog == null ? "remind id is not found" : "");
                return NotFound(result);
            }
            todoList.ProcessFlag = 1;
            todoList.ProcessedAt = DateTime.Now;
            remindLog.ProcessFlag = 1;

            try
            {
                await _pegasusContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            return Ok(result);  //Should return a view page
        }

        // PUT: api/Session/5
        [HttpPut("{sessionId}/{reason}/{userId}")]
        public async Task<IActionResult> PutSesson(int sessionId, string reason, short userId)
        {
            var result = new Result<string>();
            Lesson lesson = await _pegasusContext.Lesson.Where(x => x.LessonId == sessionId).FirstOrDefaultAsync();

            if (lesson == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Session not found";
                return NotFound(result);
            }
            if (string.IsNullOrEmpty(reason))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Reason is required";
                return NotFound(result);
            }
            var user = await _pegasusContext.User.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User id not found";
                return NotFound(result);
            }

            lesson.IsCanceled = 1;
            lesson.Reason = reason;

            bool isGroupCourse = lesson.LearnerId == null;
            var teacher = await _pegasusContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == lesson.TeacherId);
            var courses = isGroupCourse ? (from c in _pegasusContext.Course
                                           join gc in _pegasusContext.GroupCourseInstance on c.CourseId equals gc.CourseId
                                           where gc.GroupCourseInstanceId == lesson.GroupCourseInstanceId
                                           select new Course
                                           {
                                               CourseId = c.CourseId,
                                               CourseName = c.CourseName
                                           }).ToList() :
                                           (from c in _pegasusContext.Course
                                            join oto in _pegasusContext.One2oneCourseInstance on c.CourseId equals oto.CourseId
                                            where oto.CourseInstanceId == lesson.CourseInstanceId
                                            select new Course
                                            {
                                                CourseId = c.CourseId,
                                                CourseName = c.CourseName
                                            }).ToList();
            if (courses.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Fail to found course name under this lesson";
                return NotFound(DataNotFound(result));
            }
            string courseName = courses[0].CourseName;

            List<Holiday> holidays = await _pegasusContext.Holiday.ToListAsync();
            DateTime todoDate = lesson.BeginTime.Value.AddDays(-1);
            foreach(var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }
            }

            string userConfirmUrlPrefix = "https://localhost:44304/api/session/sessioncancelconfirm/"; //change it later

            if (!isGroupCourse)
                // Case of one to one course
            {
                var learner = await _pegasusContext.Learner.FirstOrDefaultAsync(l => l.LearnerId == lesson.LearnerId);
                
                TodoList todolistForTeacher = TodoListForTeacherCreater(lesson, userId, teacher, reason, todoDate);
                TodoList todolistForLearner = TodoListForLearnerCreater(lesson, userId, learner, reason, todoDate);
                RemindLog remindLogForTeacher = RemindLogForTeacherCreater(lesson, teacher, courseName, reason);
                RemindLog remindLogForLearner = RemindLogForLearnerCreater(lesson, learner, courseName, reason);

                try
                {
                    await _pegasusContext.TodoList.AddAsync(todolistForLearner);
                    await _pegasusContext.TodoList.AddAsync(todolistForTeacher);
                    await _pegasusContext.RemindLog.AddAsync(remindLogForTeacher);
                    await _pegasusContext.RemindLog.AddAsync(remindLogForLearner);
                    await _pegasusContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    result.ErrorMessage = e.Message;
                    result.IsSuccess = false;
                    return BadRequest(result);
                }
                //sending Email
                string mailTitle = "Lesson Cancellation Confirm";
                string confirmURLForLearner = userConfirmUrlPrefix + todolistForLearner.ListId + "/" + remindLogForLearner.RemindId;
                string mailContentForLearner = MailContentGenerator(learner.FirstName + " " + learner.LastName, courseName, lesson, reason, confirmURLForLearner);
                Task learnerMailSenderTask = MailSenderService.SendMailAsync(remindLogForLearner.Email, mailTitle, mailContentForLearner, remindLogForLearner.RemindId);
                string confirmURLForTeacher = userConfirmUrlPrefix + todolistForTeacher.ListId + "/" + remindLogForTeacher.RemindId;
                string mailContentForTeacher = MailContentGenerator(teacher.FirstName + " " + teacher.LastName, courseName, lesson, reason, confirmURLForTeacher);
                Task teacherMailSenderTask = MailSenderService.SendMailAsync(remindLogForTeacher.Email, mailTitle, mailContentForTeacher, remindLogForTeacher.RemindId);
            }
            else
            //Case of group course
            {
                var learners = (from lgc in _pegasusContext.LearnerGroupCourse
                                join l in _pegasusContext.Learner on lgc.LearnerId equals l.LearnerId
                                where lgc.GroupCourseInstanceId == lesson.GroupCourseInstanceId
                                select new Learner()
                                {
                                    LearnerId = l.LearnerId,
                                    FirstName = l.FirstName,
                                    LastName = l.LastName,
                                    Email = l.Email
                                }).ToList();

                TodoList todolistForTeacher = TodoListForTeacherCreater(lesson, userId, teacher, reason, todoDate);
                List<TodoList> todolistForLearners = TodoListForLearnerCreater(lesson, userId, learners, reason, todoDate);
                RemindLog remindLogForTeacher = RemindLogForTeacherCreater(lesson, teacher, courseName, reason);
                List<RemindLog> remindLogForLearners = RemindLogForLearnerCreater(lesson, learners, courseName, reason);

                try
                {
                    await _pegasusContext.TodoList.AddAsync(todolistForTeacher);
                    await _pegasusContext.RemindLog.AddAsync(remindLogForTeacher);
                    foreach (var todolist in todolistForLearners)
                    {
                        await _pegasusContext.TodoList.AddAsync(todolist);
                    }
                    foreach (var remindLog in remindLogForLearners)
                    {
                        await _pegasusContext.RemindLog.AddAsync(remindLog);
                    }
                    await _pegasusContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    result.ErrorMessage = e.Message;
                    result.IsSuccess = false;
                    return BadRequest(result);
                }
                //sending Email
                string mailTitle = "Lesson Cancellation Confirm";
                List<Task> learnerMailSenderTasks = new List<Task>();
                for (int i = 0; i < remindLogForLearners.Count; i++)
                {
                    string confirmURLForLearner = userConfirmUrlPrefix + todolistForLearners[i].ListId + "/" + remindLogForLearners[0].RemindId;
                    string mailContentForLearner = MailContentGenerator(learners[i].FirstName + " " + learners[i].LastName, courseName, lesson, reason, confirmURLForLearner);
                    learnerMailSenderTasks.Add(MailSenderService.SendMailAsync(remindLogForLearners[i].Email, mailTitle, mailContentForLearner, remindLogForLearners[i].RemindId));
                }
                string confirmURLForTeacher = userConfirmUrlPrefix + todolistForTeacher.ListId + "/" + remindLogForTeacher.RemindId;
                string mailContentForTeacher = MailContentGenerator(teacher.FirstName + " " + teacher.LastName, courseName, lesson, reason, confirmURLForTeacher);
                Task teacherMailSenderTask = MailSenderService.SendMailAsync(remindLogForTeacher.Email, mailTitle, mailContentForTeacher, remindLogForTeacher.RemindId);
            }
            return Ok(result);
        }

        private bool LessonExists(int id)
        {
            return _pegasusContext.Lesson.Any(e => e.LessonId == id);
        }

        private TodoList TodoListForTeacherCreater(Lesson lesson, short userId, Teacher teacher, string reason, DateTime todoDate)
        {
            TodoList todolistForTeacher = new TodoList();
            todolistForTeacher.ListName = "Cancellation to Remind";
            todolistForTeacher.ListContent = "Inform teacher " + teacher.FirstName + " " + teacher.LastName + 
                " session from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() + 
                " has been cancelled due to " + reason;
            todolistForTeacher.CreatedAt = DateTime.Now;
            todolistForTeacher.ProcessedAt = null;
            todolistForTeacher.ProcessFlag = 0;
            todolistForTeacher.UserId = userId;
            todolistForTeacher.TodoDate = todoDate;
            todolistForTeacher.LearnerId = null;
            todolistForTeacher.LessonId = lesson.LessonId;
            todolistForTeacher.TeacherId = lesson.TeacherId;
            return todolistForTeacher;
        }

        private RemindLog RemindLogForTeacherCreater(Lesson lesson, Teacher teacher, string courseName, string reason)
        {
            RemindLog remindLogTeacher = new RemindLog();
            remindLogTeacher.LearnerId = null;
            remindLogTeacher.Email = teacher.Email;
            remindLogTeacher.RemindType = 1;
            remindLogTeacher.RemindContent = "Your " + courseName + " lesson from " + lesson.BeginTime.ToString() +
                " to " + lesson.EndTime.ToString() + " has been cancelled due to " + reason +
                "\n Please click the following link to confirm. \n";
            remindLogTeacher.CreatedAt = DateTime.Now;
            remindLogTeacher.TeacherId = lesson.TeacherId;
            remindLogTeacher.IsLearner = 0;
            remindLogTeacher.ProcessFlag = 0;
            remindLogTeacher.EmailAt = null;
            remindLogTeacher.RemindTitle = "Lesson Cancellation Remind";
            remindLogTeacher.ReceivedFlag = 0;
            remindLogTeacher.LessonId = lesson.LessonId;
            return remindLogTeacher;
        }

        private List<TodoList> TodoListForLearnerCreater(Lesson lesson, short userId, List<Learner> learners, string reason, DateTime todoDate)
        {
            List<TodoList> todoLists = new List<TodoList>();
            foreach(var learner in learners)
            {
                TodoList todolistForLearner = TodoListForLearnerCreater(lesson, userId, learner, reason, todoDate);
                todoLists.Add(todolistForLearner);
            }
            return todoLists;
        }

        private List<RemindLog> RemindLogForLearnerCreater(Lesson lesson, List<Learner> learners, string courseName, string reason)
        {
            List<RemindLog> remindLogs = new List<RemindLog>();
            foreach(var learner in learners)
            {
                RemindLog remindLogLearner = RemindLogForLearnerCreater(lesson, learner, courseName, reason);
                remindLogs.Add(remindLogLearner);
            }
            return remindLogs;
        }

        private TodoList TodoListForLearnerCreater(Lesson lesson, short userId, Learner learner, string reason, DateTime todoDate)
        {
            TodoList todolistForLearner = new TodoList();
            todolistForLearner.ListName = "Cancellation to Remind";
            todolistForLearner.ListContent = "Inform learner " + learner.FirstName + " " + learner.LastName +
                " session from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been cancelled due to " + reason; todolistForLearner.CreatedAt = DateTime.Now;
            todolistForLearner.ProcessedAt = null;
            todolistForLearner.ProcessFlag = 0;
            todolistForLearner.UserId = userId;
            todolistForLearner.TodoDate = todoDate;
            todolistForLearner.LearnerId = learner.LearnerId;
            todolistForLearner.LessonId = lesson.LessonId;
            todolistForLearner.TeacherId = null;
            return todolistForLearner;
        }

        private RemindLog RemindLogForLearnerCreater(Lesson lesson, Learner learner, string courseName, string reason)
        {
            RemindLog remindLogLearner = new RemindLog();
            remindLogLearner.LearnerId = learner.LearnerId;
            remindLogLearner.Email = learner.Email;
            remindLogLearner.RemindType = 1;
            remindLogLearner.RemindContent = "Your " + courseName + " lesson from " + lesson.BeginTime.ToString() +
                            " to " + lesson.EndTime.ToString() + " has been cancelled due to " + reason +
                            "\n Please click the following link to confirm. \n"; remindLogLearner.CreatedAt = DateTime.Now;
            remindLogLearner.TeacherId = null;
            remindLogLearner.IsLearner = 1;
            remindLogLearner.ProcessFlag = 0;
            remindLogLearner.EmailAt = null;
            remindLogLearner.RemindTitle = "Lesson Cancellation Remind";
            remindLogLearner.ReceivedFlag = 0;
            remindLogLearner.LessonId = lesson.LessonId;
            return remindLogLearner;
        }

        private string MailContentGenerator(string name, string courseName, Lesson lesson, string reason, string confirmURL)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>Your " +
                    courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                    " has been cancelled due to " + reason + ". Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }

        private void UpdateTableAfterSendingEmail(int remindLogId)
        {
            var remindLog = _pegasusContext.RemindLog.Where(r => r.RemindId == remindLogId).FirstOrDefault();
            remindLog.EmailAt = DateTime.Now;
            remindLog.ReceivedFlag = 1;
            _pegasusContext.SaveChanges();
        }
    }
}