using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public SessionController(pegasusContext.ablemusicContext ablemusicContext, IMapper mapper, IConfiguration configuration)
        {
            _ablemusicContext = ablemusicContext;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPut("[action]/{lessonId}/{reason}")]
        public async Task<IActionResult> Confirm(int lessonId, string reason)
        {
            Decimal? houlyWage = 0;
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var lesson = _ablemusicContext.Lesson
                        .Include(s=>s.GroupCourseInstance)
                        .ThenInclude(s=>s.Course)
                        .Include(s=>s.GroupCourseInstance)
                        .ThenInclude(s=>s.Course)
                        .FirstOrDefault(s => s.LessonId == lessonId);
                    if (lesson == null)
                    {
                        return NotFound(DataNotFound(result));
                    }

                    if (lesson.IsCanceled == 1)
                    {
                        throw new Exception("This lesson has already canceled");
                    }

                    if (lesson.IsConfirm == 1)
                    {
                        throw new Exception("This lesson has already confirm.");
                    }
                    
                    if (!IsNull(lesson.GroupCourseInstanceId))
                    {
                        lesson.IsConfirm = 1;
                        lesson.Reason = reason;
                        _ablemusicContext.Update(lesson);
                        await _ablemusicContext.SaveChangesAsync();
                        
                        var houlywage = _ablemusicContext.TeacherWageRates
                            .FirstOrDefault(s => s.TeacherId == lesson.TeacherId && s.IsActivate == 1).GroupRates;
                        var GroupWageAmout = (double) houlywage*(lesson.EndTime.Value.Subtract(lesson.BeginTime.Value).TotalMinutes/60);
                        var teacherTransactionForGroup = new TeacherTransaction
                        {
                            LessonId = lesson.LessonId,CreatedAt = DateTime.Now,
                            WageAmount = (decimal) GroupWageAmout,
                            TeacherId = lesson.TeacherId
                        };
                        _ablemusicContext.Add(teacherTransactionForGroup);
                        await _ablemusicContext.SaveChangesAsync();
                        dbContextTransaction.Commit();
                        result.Data = "success";
                        return Ok(result);

                    }
                    lesson.IsConfirm = 1;
                    lesson.Reason = reason;
                    _ablemusicContext.Update(lesson);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //lessonRemain
                    var lessonRemain = _ablemusicContext.LessonRemain.FirstOrDefault(s =>
                        s.LearnerId == lesson.LearnerId && s.CourseInstanceId == lesson.CourseInstanceId);
                    if (lessonRemain.Quantity == 0)
                    {
                        throw new Exception("the remain quantities of your lesson is 0");
                    }

                    lessonRemain.Quantity -= 1;
                    _ablemusicContext.Update(lessonRemain);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //fund
                    var courseInstance =
                        _ablemusicContext.One2oneCourseInstance.FirstOrDefault(s =>
                            s.CourseInstanceId == lesson.CourseInstanceId);
                    var course = _ablemusicContext.Course.FirstOrDefault(s => s.CourseId == courseInstance.CourseId);
                    var fund = _ablemusicContext.Fund.FirstOrDefault(s => s.LearnerId == lesson.LearnerId);
                    fund.Balance -= course.Price;
                    _ablemusicContext.Update(fund);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //learner transaction
                    var learnerTransaction = new LearnerTransaction
                    {
                        LessonId = lesson.LessonId,CreatedAt = DateTime.Now.ToShortDateString(),
                        Amount = course.Price.ToString(),LearnerId = lesson.LearnerId
                    };
                    _ablemusicContext.Add(learnerTransaction);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //teacher transaction
                    var teacherWageRate =
                        _ablemusicContext.TeacherWageRates.FirstOrDefault(s =>
                            s.TeacherId == lesson.TeacherId && s.IsActivate == 1);
                    
                    var courseCatogoryId = lesson.CourseInstance.Course.CourseCategoryId;
                    if (courseCatogoryId == 1)
                    {
                        houlyWage = teacherWageRate.PianoRates;
                    }

                    else if (courseCatogoryId == 7)
                    {
                        houlyWage = teacherWageRate.TheoryRates;
                    }
                    else
                    {
                        houlyWage = teacherWageRate.OthersRates;
                    }
                    var wageAmout = (double) houlyWage*(lesson.EndTime.Value.Subtract(lesson.BeginTime.Value).TotalMinutes/60);
                    var teacherTransaction = new TeacherTransaction
                    {
                        LessonId = lesson.LessonId,CreatedAt = DateTime.Now,
                        WageAmount = (decimal) wageAmout,
                        TeacherId = lesson.TeacherId
                    };
                    _ablemusicContext.Add(teacherTransaction);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    dbContextTransaction.Commit();
                    result.Data = "Success";
                    return Ok(result);

                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        }
        
        [Route("sessionCancelConfirm/{listId}/{remindId}")]
        [HttpGet]
        public async Task<IActionResult> CancelConfirm(int listId, int remindId)
        {
            var result = new Result<string>();
            TodoList todoList;
            RemindLog remindLog;
            try
            {
                todoList = await _ablemusicContext.TodoList.Where(t => t.ListId == listId).FirstOrDefaultAsync();
                remindLog = await _ablemusicContext.RemindLog.Where(r => r.RemindId == remindId).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return NotFound(result);
            }
            
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
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            return Ok(result);  //Should redirect to a success page!
        }

        [Route("sessionCancelConfirm/{remindId}")]
        [HttpGet]
        public async Task<IActionResult> CancelConfirm(int remindId)
        {
            var result = new Result<string>();
            RemindLog remindLog;
            try
            {
                remindLog = await _ablemusicContext.RemindLog.Where(r => r.RemindId == remindId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return NotFound(result);
            }

            if (remindLog == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "remind id is not found";
                return NotFound(result);
            }
            remindLog.ProcessFlag = 1;

            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            return Ok(result);  //Should redirect to a success page!
        }

        // PUT: api/Session/5
        [HttpPut("{sessionId}/{reason}/{userId}")]
        public async Task<IActionResult> PutSesson(int sessionId, string reason, short userId)
        {
            var result = new Result<string>();
            Lesson lesson;
            try
            {
                lesson = await _ablemusicContext.Lesson.Where(x => x.LessonId == sessionId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return NotFound(result);
            }
             
            if (lesson == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Session not found";
                return NotFound(result);
            }
            if(lesson.IsCanceled == 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "This lesson has already been cancelled";
                return BadRequest(result);
            }
            if (string.IsNullOrEmpty(reason))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Reason is required";
                return NotFound(result);
            }
            var user = await _ablemusicContext.User.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User id not found";
                return NotFound(result);
            }

            lesson.IsCanceled = 1;
            lesson.Reason = reason;

            bool isGroupCourse = lesson.LearnerId == null;
            Teacher teacher;
            List<Course> courses;
            List<Holiday> holidays;
            try
            {
                teacher = await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == lesson.TeacherId);
                courses = isGroupCourse ? (from c in _ablemusicContext.Course
                                               join gc in _ablemusicContext.GroupCourseInstance on c.CourseId equals gc.CourseId
                                               where gc.GroupCourseInstanceId == lesson.GroupCourseInstanceId
                                               select new Course
                                               {
                                                   CourseId = c.CourseId,
                                                   CourseName = c.CourseName
                                               }).ToList() :
                                               (from c in _ablemusicContext.Course
                                                join oto in _ablemusicContext.One2oneCourseInstance on c.CourseId equals oto.CourseId
                                                where oto.CourseInstanceId == lesson.CourseInstanceId
                                                select new Course
                                                {
                                                    CourseId = c.CourseId,
                                                    CourseName = c.CourseName
                                                }).ToList();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return NotFound(result);
            }
            
            if (courses.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Fail to found course name under this lesson";
                return NotFound(result);
            }
            string courseName = courses[0].CourseName;

            DateTime todoDate = lesson.BeginTime.Value.AddDays(-1);
            foreach(var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }                
            }

            //string userConfirmUrlPrefix = "https://localhost:44304/api/session/sessioncancelconfirm/"; 
            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;

            if (!isGroupCourse)
                // Case of one to one course
            {
                Learner learner;
                try
                {
                    learner = await _ablemusicContext.Learner.FirstOrDefaultAsync(l => l.LearnerId == lesson.LearnerId);
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.ToString();
                    return NotFound(result);
                }
                
                TodoList todolistForTeacher = TodoListForTeacherCreater(lesson, userId, teacher, reason, todoDate);
                TodoList todolistForLearner = TodoListForLearnerCreater(lesson, userId, learner, reason, todoDate);
                RemindLog remindLogForTeacher = RemindLogForTeacherCreater(lesson, teacher, courseName, reason);
                RemindLog remindLogForLearner = RemindLogForLearnerCreater(lesson, learner, courseName, reason);

                try
                {
                    await _ablemusicContext.TodoList.AddAsync(todolistForLearner);
                    await _ablemusicContext.TodoList.AddAsync(todolistForTeacher);
                    await _ablemusicContext.RemindLog.AddAsync(remindLogForTeacher);
                    await _ablemusicContext.RemindLog.AddAsync(remindLogForLearner);
                    await _ablemusicContext.SaveChangesAsync();
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
                List<Learner> learners;
                try
                {
                    learners = (from lgc in _ablemusicContext.LearnerGroupCourse
                                join l in _ablemusicContext.Learner on lgc.LearnerId equals l.LearnerId
                                where lgc.GroupCourseInstanceId == lesson.GroupCourseInstanceId
                                select new Learner()
                                {
                                    LearnerId = l.LearnerId,
                                    FirstName = l.FirstName,
                                    LastName = l.LastName,
                                    Email = l.Email
                                }).ToList();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.ToString();
                    return NotFound(result);
                }

                TodoList todolistForTeacher = TodoListForTeacherCreater(lesson, userId, teacher, reason, todoDate);
                List<TodoList> todolistForLearners = TodoListForLearnerCreater(lesson, userId, learners, reason, todoDate);
                RemindLog remindLogForTeacher = RemindLogForTeacherCreater(lesson, teacher, courseName, reason);
                List<RemindLog> remindLogForLearners = RemindLogForLearnerCreater(lesson, learners, courseName, reason);

                try
                {
                    await _ablemusicContext.TodoList.AddAsync(todolistForTeacher);
                    await _ablemusicContext.RemindLog.AddAsync(remindLogForTeacher);
                    foreach (var todolist in todolistForLearners)
                    {
                        await _ablemusicContext.TodoList.AddAsync(todolist);
                    }
                    foreach (var remindLog in remindLogForLearners)
                    {
                        await _ablemusicContext.RemindLog.AddAsync(remindLog);
                    }
                    await _ablemusicContext.SaveChangesAsync();
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
                    string confirmURLForLearner = userConfirmUrlPrefix + todolistForLearners[i].ListId + "/" + remindLogForLearners[i].RemindId;
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
            return _ablemusicContext.Lesson.Any(e => e.LessonId == id);
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

    }
}