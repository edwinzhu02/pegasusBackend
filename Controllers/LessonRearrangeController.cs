using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;
using Pegasus_backend.Repositories;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonRearrangeController : BasicController
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public LessonRearrangeController(ablemusicContext ablemusicContext, ILogger<LessonRearrangeController> log, IMapper mapper, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _mapper = mapper;
            _configuration = configuration;
        }

        // PUT: api/LessonRearrange/5
        [HttpPut("{userId}")]
        [CheckModelFilter]
        public async Task<IActionResult> PutLesson(short userId, [FromBody] LessonViewModel lessonViewmodel)
        {
            var result = new Result<Lesson>();
            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;
            Lesson newLesson = new Lesson();
            Lesson oldLesson = new Lesson();
            
            _mapper.Map(lessonViewmodel, newLesson);
            try
            {
                oldLesson = await _ablemusicContext.Lesson.Where(l => l.LessonId == newLesson.LessonId).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsFound = false;
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return NotFound(result);
            }
            if(oldLesson == null)
            {
                result.IsFound = false;
                result.IsSuccess = false;
                result.ErrorMessage = "Lesson id not found";
                return NotFound(result);
            }

            TimeSpan lessonDuration = oldLesson.EndTime.Value.Subtract(oldLesson.BeginTime.Value);
            newLesson.EndTime = newLesson.BeginTime.Value.Add(lessonDuration);

            var lessonConflictCheckerService = new LessonConflictCheckerService(_ablemusicContext, newLesson.BeginTime.Value, 
                newLesson.EndTime.Value, newLesson.RoomId.Value, newLesson.OrgId, (int)newLesson.TeacherId, oldLesson.LessonId);
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

            Learner learner = new Learner();
            List<Learner> learners = new List<Learner>();
            Teacher newTeacher;
            Teacher oldTeacher;
            pegasusContext.Org oldOrg;
            pegasusContext.Org newOrg;
            Room oldRoom;
            Room newRoom;
            Course course;
            List<Holiday> holidays;
            try
            {
                if(oldLesson.CourseInstanceId != null)
                {
                    learner = await _ablemusicContext.Learner.FirstOrDefaultAsync(l => l.LearnerId == oldLesson.LearnerId);
                    course = await (from c in _ablemusicContext.Course
                                    join oto in _ablemusicContext.One2oneCourseInstance on c.CourseId equals oto.CourseId
                                    join l in _ablemusicContext.Lesson on oto.CourseInstanceId equals l.CourseInstanceId
                                    where l.LessonId == newLesson.LessonId
                                    select new Course
                                    {
                                        CourseId = c.CourseId,
                                        CourseName = c.CourseName
                                    }).FirstOrDefaultAsync();
                }
                else
                {
                    learners = await (from gc in _ablemusicContext.GroupCourseInstance
                                      join lgc in _ablemusicContext.LearnerGroupCourse on gc.GroupCourseInstanceId equals lgc.GroupCourseInstanceId
                                      join l in _ablemusicContext.Learner on lgc.LearnerId equals l.LearnerId
                                      where gc.GroupCourseInstanceId == oldLesson.GroupCourseInstanceId
                                      select l
                                      ).ToListAsync();
                    course = await (from c in _ablemusicContext.Course
                                    join gc in _ablemusicContext.GroupCourseInstance on c.CourseId equals gc.CourseId
                                    join l in _ablemusicContext.Lesson on gc.GroupCourseInstanceId equals l.GroupCourseInstanceId
                                    where l.LessonId == newLesson.LessonId
                                    select new Course
                                    {
                                        CourseId = c.CourseId,
                                        CourseName = c.CourseName
                                    }).FirstOrDefaultAsync();
                }
                newTeacher = await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == newLesson.TeacherId);
                oldTeacher = newLesson.TeacherId == oldLesson.TeacherId ? newTeacher : 
                    await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == oldLesson.TeacherId);
                if(oldLesson.IsTrial == 1)
                {
                    course = await _ablemusicContext.Course.Where(c => c.CourseId == oldLesson.TrialCourseId).FirstOrDefaultAsync();
                }
                holidays = await _ablemusicContext.Holiday.ToListAsync();
                oldOrg = await _ablemusicContext.Org.Where(o => o.OrgId == oldLesson.OrgId).FirstOrDefaultAsync();
                newOrg = newLesson.OrgId == oldLesson.OrgId ? oldOrg :
                    await _ablemusicContext.Org.Where(o => o.OrgId == newLesson.OrgId).FirstOrDefaultAsync();
                oldRoom = await _ablemusicContext.Room.Where(r => r.RoomId == oldLesson.RoomId).FirstOrDefaultAsync();
                newRoom = newLesson.RoomId == oldLesson.RoomId ? oldRoom :
                    await _ablemusicContext.Room.Where(r => r.RoomId == newLesson.RoomId).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return NotFound(result);
            }
            if (course == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Fail to found course name under this lesson";
                return NotFound(result);
            }
            string courseName = course.CourseName;

            DateTime oldTodoDate = oldLesson.BeginTime.Value.AddDays(-1);
            DateTime newTodoDate = newLesson.BeginTime.Value.AddDays(-1);

            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == oldTodoDate.Date)
                {
                    oldTodoDate = oldTodoDate.AddDays(-1);
                }
                if (holiday.HolidayDate.Date == newTodoDate.Date)
                {
                    newTodoDate = newTodoDate.AddDays(-1);
                }
            }

            DateTime oldRemindScheduledDate = oldTodoDate;
            DateTime newRemindScheduledDate = newTodoDate;

            oldLesson.IsCanceled = 1;
            oldLesson.IsChanged = 1;
            oldLesson.NewLessonId = 1;            
            oldLesson.Reason = newLesson.Reason;

            newLesson.LessonId = 0;
            newLesson.LearnerId = oldLesson.LearnerId;
            newLesson.IsCanceled = 0;
            newLesson.Reason = "";
            newLesson.CreatedAt = toNZTimezone(DateTime.UtcNow);
            newLesson.CourseInstanceId = oldLesson.CourseInstanceId;
            newLesson.GroupCourseInstanceId = oldLesson.GroupCourseInstanceId;
            newLesson.IsTrial = oldLesson.IsTrial;
            newLesson.TrialCourseId = oldLesson.TrialCourseId;
            newLesson.InvoiceNum = oldLesson.InvoiceNum;
            newLesson.IsPaid = oldLesson.IsPaid;
            //newLesson.NewLessonId = oldLesson.NewLessonId;            
            newLesson.IsChanged = 0;

            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                try
                {
                    await _ablemusicContext.Lesson.AddAsync(newLesson);
                    oldLesson.NewLessonId = newLesson.LessonId;  
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.ToString();
                    return BadRequest(result);
                }

                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                if (oldLesson.CourseInstanceId != null)
                {
                    todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(
                    oldLesson, newLesson, learner, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, newTodoDate, newLesson.LessonId, learner.LearnerId, null);
                    todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(
                    oldLesson, newLesson, learner, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, oldTodoDate, oldLesson.LessonId, learner.LearnerId, null);
                }
                else
                {
                    Dictionary<int, string> learnerIdMapContent = new Dictionary<int, string>();
                    foreach (var l in learners)
                    {
                        learnerIdMapContent.Add(l.LearnerId, TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(oldLesson, newLesson, l, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom));
                    }
                    todoRepository.AddMutipleTodoLists("Lesson Rearrangement to Remind", learnerIdMapContent, userId, newTodoDate, newLesson.LessonId, null);
                    todoRepository.AddMutipleTodoLists("Lesson Rearrangement to Remind", learnerIdMapContent, userId, oldTodoDate, newLesson.LessonId, null);
                }
                todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForOldTeacher(
                    oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, newTodoDate, newLesson.LessonId, null, oldTeacher.TeacherId);
                todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForNewTeacher(
                    oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, newTodoDate, newLesson.LessonId, null, newTeacher.TeacherId);

                todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForOldTeacher(
                    oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, oldTodoDate, oldLesson.LessonId, null, oldTeacher.TeacherId);
                todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForNewTeacher(
                    oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, oldTodoDate, oldLesson.LessonId, null, newTeacher.TeacherId);
                var saveTodoResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoResult.IsSuccess)
                {
                    return BadRequest(saveTodoResult);
                }

                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                DateTime earlyDate = newRemindScheduledDate >= oldRemindScheduledDate ? oldRemindScheduledDate : newRemindScheduledDate;
                DateTime laterDate = newRemindScheduledDate >= oldRemindScheduledDate ? newRemindScheduledDate : oldRemindScheduledDate;
                DateTime laterDateNoticeWeekBefore = laterDate.AddDays(-7);
                int dateDifferentBetweenNowAndEarlyDate = (int)(earlyDate - DateTime.UtcNow.ToNZTimezone()).TotalDays;
                int dateDifferentBetweenEarlyDateAndLaterDate = (int)(laterDate - earlyDate).TotalDays;
                if(dateDifferentBetweenNowAndEarlyDate < 7 & dateDifferentBetweenNowAndEarlyDate > 1)
                {
                    earlyDate = DateTime.UtcNow.ToNZTimezone().AddDays(1);
                }
                List<DateTime> remindScheduledDates = new List<DateTime>
                {
                    earlyDate,
                    laterDate
                };
                foreach(var remindScheduleDate in remindScheduledDates)
                {
                    if (oldLesson.CourseInstanceId != null)
                    {
                        remindLogRepository.AddSingleRemindLog(learner.LearnerId, learner.Email, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(
                        oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName), null, "Lesson Rearrangement Remind", newLesson.LessonId, remindScheduleDate);
                    }
                    else
                    {
                        Dictionary<Learner, string> learnerMapContent = new Dictionary<Learner, string>();
                        foreach (var l in learners)
                        {
                            learnerMapContent.Add(l, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName));
                        }
                        remindLogRepository.AddMultipleRemindLogs(learnerMapContent, null, "Lesson rearrangement Remind", newLesson.LessonId, remindScheduleDate);
                    }
                    remindLogRepository.AddSingleRemindLog(null, oldTeacher.Email, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForOldTeacher(
                        oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName), oldTeacher.TeacherId, "Lesson Rearrangement Remind", oldLesson.LessonId, remindScheduleDate);
                    remindLogRepository.AddSingleRemindLog(null, newTeacher.Email, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForNewTeacher(
                        oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName), newTeacher.TeacherId, "Lesson Rearrangement Remind", newLesson.LessonId, remindScheduleDate);
                }

                foreach(var remindlog in remindLogRepository._remindLogs.Reverse<RemindLog>())
                {
                    if (remindlog.ScheduledDate <= DateTime.UtcNow.ToNZTimezone())
                    {
                        remindLogRepository._remindLogs.Remove(remindlog);
                    }
                    if (dateDifferentBetweenEarlyDateAndLaterDate <= 7)
                    {
                        if (remindlog.ScheduledDate.Value.Date == laterDateNoticeWeekBefore.Date)
                        {
                            remindLogRepository._remindLogs.Remove(remindlog);
                        }
                    }
                }

                var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindLogResult.IsSuccess)
                {
                    try
                    {
                        oldLesson.IsCanceled = 0;
                        oldLesson.Reason = "";
                        _ablemusicContext.Lesson.Remove(newLesson);
                        await _ablemusicContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = ex.Message + "\n" + saveTodoResult.ErrorMessage;
                        return BadRequest(result);
                    }
                    return BadRequest(saveRemindLogResult);
                }

                //sending Email
                //List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
                foreach (var remind in saveRemindLogResult.Data)
                {
                    var todo = saveTodoResult.Data.Find(t => t.LearnerId == remind.LearnerId && t.TeacherId == remind.TeacherId && t.TodoDate == newTodoDate);
                    string currentPersonName;
                    if (remind.TeacherId == null)
                    {
                        currentPersonName = learner.FirstName + " " + learner.LastName;
                    }
                    else
                    {
                        currentPersonName = remind.TeacherId == newTeacher.TeacherId ? newTeacher.FirstName + " " + newTeacher.LastName : oldTeacher.FirstName + " " + oldTeacher.LastName;
                    }
                    string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                    string mailContent = EmailContentGenerator.RearrangeLessonWithOld(currentPersonName, courseName, confirmURL, oldLesson, oldTeacher,
                        newTeacher, oldOrg, newOrg, oldRoom, newRoom);
                    remindLogRepository.UpdateContent(remind.RemindId, mailContent);
                    //notifications.Add(new NotificationEventArgs(remind.Email, "Lesson Rearrange Confirm", mailContent, remind.RemindId));
                }
                var remindLogUpdateContentResult = await remindLogRepository.SaveUpdatedContentAsync();
                if (!remindLogUpdateContentResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = remindLogUpdateContentResult.ErrorMessage;
                    return BadRequest(result);
                }
                //foreach (var mail in notifications)
                //{
                //    _notificationObservable.send(mail);
                //}
                dbContextTransaction.Commit();
            }

            result.IsSuccess = true;
            //result.Data = newLesson;
            return Ok(result);
        }

        [HttpPut("{awaitId}/{days}")]
        public async Task<IActionResult> ChangeExpiryDate(int awaitId,short days)
        {
            var result = new Result<object>();
            try
            {
                var makeUpLesson = await _ablemusicContext.AwaitMakeUpLesson.
                    FirstOrDefaultAsync(t => t.AwaitId == awaitId);
                if ( makeUpLesson==null )
                    throw new Exception("No such lesson!");
                makeUpLesson.ExpiredDate = makeUpLesson.ExpiredDate.Value.AddDays(days);    
                _ablemusicContext.Update(makeUpLesson);
                result.Data =  makeUpLesson;      
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);;
        }
        private bool LessonExists(int id)
        {
            return _ablemusicContext.Lesson.Any(e => e.LessonId == id);
        }
    }
}