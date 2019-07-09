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
            var result = new Result<string>();
            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;
            Lesson newLesson = new Lesson();
            Lesson oldLesson = new Lesson();
            Learner learner = new Learner();
            _mapper.Map(lessonViewmodel, newLesson);
            try
            {
                oldLesson = await _ablemusicContext.Lesson.Where(l => l.LessonId == newLesson.LessonId).FirstOrDefaultAsync();
                learner = await _ablemusicContext.Learner.FirstOrDefaultAsync(l => l.LearnerId == oldLesson.LearnerId);
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
            if(oldLesson.GroupCourseInstanceId != null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Goupe course is not allow to rearrange";
                return BadRequest(result);
            }
            TimeSpan lessonDuration = oldLesson.EndTime.Value.Subtract(oldLesson.BeginTime.Value);
            newLesson.EndTime = newLesson.BeginTime.Value.Add(lessonDuration);

            var lessonConflictCheckerService = new LessonConflictCheckerService(newLesson.BeginTime.Value, 
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
                newTeacher = await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == newLesson.TeacherId);
                oldTeacher = newLesson.TeacherId == oldLesson.TeacherId ? newTeacher : 
                    await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == oldLesson.TeacherId);
                if(oldLesson.IsTrial == 1)
                {
                    course = await _ablemusicContext.Course.Where(c => c.CourseId == oldLesson.TrialCourseId).FirstOrDefaultAsync();
                } else
                {
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

            oldLesson.IsCanceled = 1;
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
            newLesson.InvoiceId = oldLesson.InvoiceId;
            newLesson.IsChanged = 1;

            try
            {
                await _ablemusicContext.Lesson.AddAsync(newLesson);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            TodoRepository todoRepository = new TodoRepository();
            todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(
                oldLesson, newLesson, learner, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, newTodoDate, newLesson.LessonId, learner.LearnerId, null);
            todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForOldTeacher(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, newTodoDate, newLesson.LessonId, null, oldTeacher.TeacherId);
            todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForNewTeacher(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, newTodoDate, newLesson.LessonId, null, newTeacher.TeacherId);
            todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(
                oldLesson, newLesson, learner, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, oldTodoDate, oldLesson.LessonId, learner.LearnerId, null);
            todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForOldTeacher(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, oldTodoDate, oldLesson.LessonId, null, oldTeacher.TeacherId);
            todoRepository.AddSingleTodoList("Lesson Rearrangement to Remind", TodoListContentGenerator.RearrangedSingleLessonWithOldLessonForNewTeacher(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom), userId, oldTodoDate, oldLesson.LessonId, null, newTeacher.TeacherId);
            var saveTodoResult = await todoRepository.SaveTodoListsAsync();
            if (!saveTodoResult.IsSuccess)
            {
                try
                {
                    oldLesson.IsCanceled = 0;
                    oldLesson.Reason = "";
                    _ablemusicContext.Lesson.Remove(newLesson);
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message + "\n" + saveTodoResult.ErrorMessage;
                    return BadRequest(result);
                }
                return BadRequest(saveTodoResult);
            }

            RemindLogRepository remindLogRepository = new RemindLogRepository();
            remindLogRepository.AddSingleRemindLog(learner.LearnerId, learner.Email, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForLearner(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName), null, "Lesson Rearrangement Remind", newLesson.LessonId);
            remindLogRepository.AddSingleRemindLog(null, oldTeacher.Email, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForOldTeacher(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName), oldTeacher.TeacherId, "Lesson Rearrangement Remind", oldLesson.LessonId);
            remindLogRepository.AddSingleRemindLog(null, newTeacher.Email, RemindLogContentGenerator.RearrangedSingleLessonWithOldLessonForNewTeacher(
                oldLesson, newLesson, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, courseName), newTeacher.TeacherId, "Lesson Rearrangement Remind", newLesson.LessonId);
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
            List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
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
                notifications.Add(new NotificationEventArgs(remind.Email, "Lesson Rearrange Confirm", mailContent, remind.RemindId));
            }
            foreach (var mail in notifications)
            {
                _notificationObservable.send(mail);
            }

            result.IsSuccess = true;
            return Ok(result);
        }

        private bool LessonExists(int id)
        {
            return _ablemusicContext.Lesson.Any(e => e.LessonId == id);
        }
    }
}