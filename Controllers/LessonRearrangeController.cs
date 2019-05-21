using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonRearrangeController : ControllerBase
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public LessonRearrangeController(pegasusContext.ablemusicContext ablemusicContext, IMapper mapper, IConfiguration configuration)
        {
            _ablemusicContext = ablemusicContext;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetLessons()
        {
            var result = new Result<List<Lesson>>();
            result.Data = await _ablemusicContext.Lesson.ToListAsync();
            return Ok(result);
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
            if(newLesson.BeginTime >= newLesson.EndTime)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Begin time must earlier than end time!";
                return BadRequest(result);
            }

            List<Lesson> conflictRooms = new List<Lesson>();
            List<Lesson> conflictTeacherLessons = new List<Lesson>();
            try
            {
                if (newLesson.RoomId != oldLesson.RoomId || newLesson.OrgId != oldLesson.OrgId ||
                newLesson.BeginTime != oldLesson.BeginTime || newLesson.EndTime != oldLesson.EndTime)
                {
                    conflictRooms = await _ablemusicContext.Lesson.Where(l => l.RoomId == newLesson.RoomId &&
                    l.OrgId == newLesson.OrgId &&
                    ((l.BeginTime > newLesson.BeginTime && l.BeginTime < newLesson.EndTime) ||
                    (l.EndTime > newLesson.BeginTime && l.EndTime < newLesson.EndTime) ||
                    (l.BeginTime <= newLesson.BeginTime && l.EndTime >= newLesson.EndTime)))
                    .ToListAsync();
                }
                if (newLesson.TeacherId != oldLesson.TeacherId || newLesson.BeginTime != oldLesson.BeginTime ||
                    newLesson.EndTime != oldLesson.EndTime)
                {
                    DateTime beginTime = newLesson.BeginTime.Value.AddMinutes(-60);
                    DateTime endTime = newLesson.EndTime.Value.AddMinutes(60);
                    conflictTeacherLessons = await _ablemusicContext.Lesson.Where(l => l.TeacherId == newLesson.TeacherId &&
                    ((l.BeginTime > beginTime && l.BeginTime < endTime) ||
                    (l.EndTime > beginTime && l.EndTime < endTime) ||
                    (l.BeginTime <= beginTime && l.EndTime >= endTime)))
                    .ToListAsync();
                }
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if(conflictRooms.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room is not available";
                return BadRequest(result);
            }
            if(conflictTeacherLessons.Count > 0)
            {
                foreach(var c in conflictTeacherLessons)
                {
                    if (c.OrgId != newLesson.OrgId ||
                        (c.BeginTime > newLesson.BeginTime && c.BeginTime < newLesson.EndTime) ||
                        (c.EndTime > newLesson.BeginTime && c.EndTime < newLesson.EndTime) ||
                        (c.BeginTime <= newLesson.BeginTime && c.EndTime >= newLesson.EndTime))
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Teacher is not available";
                        return BadRequest(result);
                    }
                }
            }

            Teacher newTeacher;
            Teacher oldTeacher;
            Pegasus_backend.pegasusContext.Org oldOrg;
            Pegasus_backend.pegasusContext.Org newOrg;
            Room oldRoom;
            Room newRoom;
            List<Course> courses;
            List<Holiday> holidays;
            try
            {
                newTeacher = await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == newLesson.TeacherId);
                oldTeacher = newLesson.TeacherId == oldLesson.TeacherId ? newTeacher : 
                    await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == oldLesson.TeacherId);
                courses = (from c in _ablemusicContext.Course
                           join oto in _ablemusicContext.One2oneCourseInstance on c.CourseId equals oto.CourseId
                           where oto.CourseInstanceId == newLesson.CourseInstanceId
                           select new Course
                           {
                               CourseId = c.CourseId,
                               CourseName = c.CourseName
                           }).ToList();
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
            if (courses.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Fail to found course name under this lesson";
                return NotFound(result);
            }
            string courseName = courses[0].CourseName;

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

            TodoList todolistForLearner = TodoListForLearnerCreater(oldLesson, newLesson, userId, learner, oldTeacher, 
                newTeacher, oldOrg,newOrg, oldRoom, newRoom, newTodoDate);
            TodoList todolistForOldTeacher = TodoListForOldTeacherCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);
            TodoList todolistForNewTeacher = TodoListForNewTeacherCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);

            TodoList todolistForLearnerAtCancelTime = TodoListForLearnerCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);
            todolistForLearnerAtCancelTime.TodoDate = oldTodoDate;
            TodoList todolistForOldTeacherAtCancelTime = TodoListForOldTeacherCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);
            todolistForOldTeacherAtCancelTime.TodoDate = oldTodoDate;
            TodoList todolistForNewTeacherAtCancelTime = TodoListForNewTeacherCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);
            todolistForNewTeacherAtCancelTime.TodoDate = oldTodoDate;

            RemindLog remindLogForLearner = RemindLogForLearnerCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate, courseName);
            RemindLog remindLogForOldTeacher = RemindLogForOldTeacherCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate, courseName);
            RemindLog remindLogForNewTeacher = RemindLogForNewTeacherCreater(oldLesson, newLesson, userId, learner, oldTeacher,
                newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate, courseName);

            try
            {
                await _ablemusicContext.TodoList.AddAsync(todolistForLearner);
                await _ablemusicContext.TodoList.AddAsync(todolistForOldTeacher);
                await _ablemusicContext.RemindLog.AddAsync(remindLogForLearner);
                await _ablemusicContext.RemindLog.AddAsync(remindLogForOldTeacher);
                if(newLesson.TeacherId != oldLesson.TeacherId)
                {
                    await _ablemusicContext.TodoList.AddAsync(todolistForNewTeacher);
                    await _ablemusicContext.RemindLog.AddAsync(remindLogForNewTeacher);
                }
                if (oldTodoDate.Date != newTodoDate.Date)
                {
                    if(newLesson.TeacherId == oldLesson.TeacherId)
                    {
                        await _ablemusicContext.TodoList.AddAsync(todolistForLearnerAtCancelTime);
                        await _ablemusicContext.TodoList.AddAsync(todolistForOldTeacherAtCancelTime);
                    } else
                    {
                        await _ablemusicContext.TodoList.AddAsync(todolistForLearnerAtCancelTime);
                        await _ablemusicContext.TodoList.AddAsync(todolistForOldTeacherAtCancelTime);
                        await _ablemusicContext.TodoList.AddAsync(todolistForNewTeacherAtCancelTime);
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            //sending Email
            string mailTitle = "Lesson Rearragement Confirm";
            string confirmURLForLearner = userConfirmUrlPrefix + todolistForLearner.ListId + "/" + remindLogForLearner.RemindId;
            string receiverName = learner.LastName + " " + learner.LastName;
            string mailContentForLearner = MailContentGenerator(receiverName, courseName, confirmURLForLearner, oldLesson, newLesson, userId, 
                learner, oldTeacher, newTeacher, oldOrg, newOrg,  oldRoom,  newRoom,  newTodoDate);
            Task learnerMailSenderTask = MailSenderService.SendMailAsync(remindLogForLearner.Email, mailTitle, mailContentForLearner, remindLogForLearner.RemindId);

            string confirmURLForOldTeacher = userConfirmUrlPrefix + todolistForOldTeacher.ListId + "/" + remindLogForOldTeacher.RemindId;
            receiverName = oldTeacher.LastName + " " + oldTeacher.LastName;
            string mailContentForOldTeacher = MailContentGenerator(receiverName, courseName, confirmURLForOldTeacher, oldLesson, newLesson, userId,
                learner, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);
            Task oldTeacherMailSenderTask = MailSenderService.SendMailAsync(remindLogForOldTeacher.Email, mailTitle, mailContentForOldTeacher, remindLogForOldTeacher.RemindId);

            if (newLesson.TeacherId != oldLesson.TeacherId)
            {
                //sending Email
                string confirmURLForNewTeacher = userConfirmUrlPrefix + todolistForNewTeacher.ListId + "/" + remindLogForNewTeacher.RemindId;
                receiverName = newTeacher.LastName + " " + newTeacher.LastName;
                string mailContentForNewTeacher = MailContentGenerator(receiverName, courseName, confirmURLForNewTeacher, oldLesson, newLesson, userId,
                    learner, oldTeacher, newTeacher, oldOrg, newOrg, oldRoom, newRoom, newTodoDate);
                Task newTeacherMailSenderTask = MailSenderService.SendMailAsync(remindLogForNewTeacher.Email, mailTitle, mailContentForNewTeacher, remindLogForNewTeacher.RemindId);
            }

            //oldLesson.LearnerId = newLesson.LearnerId;
            oldLesson.RoomId = newLesson.RoomId;
            oldLesson.TeacherId = newLesson.TeacherId;
            oldLesson.OrgId = newLesson.OrgId;
            //oldLesson.IsCanceled = newLesson.IsCanceled;
            oldLesson.Reason = newLesson.Reason;
            //oldLesson.CreatedAt = newLesson.CreatedAt;
            //oldLesson.CourseInstanceId = newLesson.CourseInstanceId;
            //oldLesson.GroupCourseInstanceId = newLesson.GroupCourseInstanceId;
            //oldLesson.IsTrial = newLesson.IsTrial;
            oldLesson.BeginTime = newLesson.BeginTime.Value;
            oldLesson.EndTime = newLesson.EndTime.Value;
            //oldLesson.InvoiceId = newLesson.InvoiceId;

            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            result.IsSuccess = true;
            return Ok(result);
        }

        private bool LessonExists(int id)
        {
            return _ablemusicContext.Lesson.Any(e => e.LessonId == id);
        }

        private TodoList TodoListForLearnerCreater(Lesson oldLesson, Lesson newLesson, short userId, Learner learner, 
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg, 
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate)
        {
            TodoList todolistForLearner = new TodoList();
            todolistForLearner.ListName = "Lesson rearrangement to Remind";
            todolistForLearner.ListContent = "Inform learner " + learner.FirstName + " " + learner.LastName +
                " session given by teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                " " + oldRoom.RoomName + " has been rearanged to be given by teacher " + newTeacher.FirstName + " " +
                newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " + newLesson.EndTime.ToString() +
                " at " + newOrg.OrgName + " " + newRoom.RoomName;
            todolistForLearner.CreatedAt = DateTime.Now;
            todolistForLearner.ProcessedAt = null;
            todolistForLearner.ProcessFlag = 0;
            todolistForLearner.UserId = userId;
            todolistForLearner.TodoDate = newTodoDate;
            todolistForLearner.LearnerId = learner.LearnerId;
            todolistForLearner.LessonId = newLesson.LessonId;
            todolistForLearner.TeacherId = null;
            return todolistForLearner;
        }

        private TodoList TodoListForNewTeacherCreater(Lesson oldLesson, Lesson newLesson, short userId, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg,
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate)
        {
            TodoList todolistForNewTeacher = new TodoList();
            todolistForNewTeacher.ListName = "Lesson rearrangement to Remind";
            todolistForNewTeacher.ListContent = "Inform teacher " + newTeacher.FirstName + " " + newTeacher.LastName +
                " session given by teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                " " + oldRoom.RoomName + " has been rearanged to be given by " + newTeacher.FirstName + " " +
                newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            todolistForNewTeacher.CreatedAt = DateTime.Now;
            todolistForNewTeacher.ProcessedAt = null;
            todolistForNewTeacher.ProcessFlag = 0;
            todolistForNewTeacher.UserId = userId;
            todolistForNewTeacher.TodoDate = newTodoDate;
            todolistForNewTeacher.LearnerId = null;
            todolistForNewTeacher.LessonId = newLesson.LessonId;
            todolistForNewTeacher.TeacherId = newTeacher.TeacherId;
            return todolistForNewTeacher;
        }

        private TodoList TodoListForOldTeacherCreater(Lesson oldLesson, Lesson newLesson, short userId, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg,
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate)
        {
            TodoList todolistForOldTeacher = new TodoList();
            todolistForOldTeacher.ListName = "Lesson rearrangement to Remind";
            todolistForOldTeacher.ListContent = "Inform teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName +
                " session given by teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                " " + oldRoom.RoomName + " has been rearanged to be given by " + newTeacher.FirstName + " " +
                newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            todolistForOldTeacher.CreatedAt = DateTime.Now;
            todolistForOldTeacher.ProcessedAt = null;
            todolistForOldTeacher.ProcessFlag = 0;
            todolistForOldTeacher.UserId = userId;
            todolistForOldTeacher.TodoDate = newTodoDate;
            todolistForOldTeacher.LearnerId = null;
            todolistForOldTeacher.LessonId = newLesson.LessonId;
            todolistForOldTeacher.TeacherId = oldTeacher.TeacherId;
            return todolistForOldTeacher;
        }

        private RemindLog RemindLogForLearnerCreater(Lesson oldLesson, Lesson newLesson, short userId, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg,
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate, string courseName)
        {
            RemindLog remindLogLearner = new RemindLog();
            remindLogLearner.LearnerId = learner.LearnerId;
            remindLogLearner.Email = learner.Email;
            remindLogLearner.RemindType = 1;
            remindLogLearner.RemindContent = "Your " + courseName + " lesson given by " + oldTeacher.FirstName + " " +
                oldTeacher.LastName + " from " + oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() +
                " at " + oldOrg.OrgName + " " + oldRoom.RoomName + " has been rearranged to be given by teacher " +
                newTeacher.FirstName + " " + newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            remindLogLearner.CreatedAt = DateTime.Now;
            remindLogLearner.TeacherId = null;
            remindLogLearner.IsLearner = 1;
            remindLogLearner.ProcessFlag = 0;
            remindLogLearner.EmailAt = null;
            remindLogLearner.RemindTitle = "Lesson Rearrangement Remind";
            remindLogLearner.ReceivedFlag = 0;
            remindLogLearner.LessonId = newLesson.LessonId;
            return remindLogLearner;
        }

        private RemindLog RemindLogForOldTeacherCreater(Lesson oldLesson, Lesson newLesson, short userId, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg,
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate, string courseName)
        {
            RemindLog remindLogForOldTeacher = new RemindLog();
            remindLogForOldTeacher.LearnerId = null;
            remindLogForOldTeacher.Email = oldTeacher.Email;
            remindLogForOldTeacher.RemindType = 1;
            remindLogForOldTeacher.RemindContent = "The " + courseName + " lesson given by " + oldTeacher.FirstName + " " +
                oldTeacher.LastName + " from " + oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() +
                " at " + oldOrg.OrgName + " " + oldRoom.RoomName + " has been rearranged to be given by teacher " +
                newTeacher.FirstName + " " + newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            remindLogForOldTeacher.CreatedAt = DateTime.Now;
            remindLogForOldTeacher.TeacherId = oldLesson.TeacherId;
            remindLogForOldTeacher.IsLearner = 0;
            remindLogForOldTeacher.ProcessFlag = 0;
            remindLogForOldTeacher.EmailAt = null;
            remindLogForOldTeacher.RemindTitle = "Lesson Rearrangement Remind";
            remindLogForOldTeacher.ReceivedFlag = 0;
            remindLogForOldTeacher.LessonId = oldLesson.LessonId;
            return remindLogForOldTeacher;
        }

        private RemindLog RemindLogForNewTeacherCreater(Lesson oldLesson, Lesson newLesson, short userId, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg,
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate, string courseName)
        {
            RemindLog remindLogForNewTeacher = new RemindLog();
            remindLogForNewTeacher.LearnerId = null;
            remindLogForNewTeacher.Email = newTeacher.Email;
            remindLogForNewTeacher.RemindType = 1;
            remindLogForNewTeacher.RemindContent = "The " + courseName + " lesson given by " + oldTeacher.FirstName + " " +
                oldTeacher.LastName + " from " + oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() +
                " at " + oldOrg.OrgName + " " + oldRoom.RoomName + " has been rearranged to be given by teacher " +
                newTeacher.FirstName + " " + newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            remindLogForNewTeacher.CreatedAt = DateTime.Now;
            remindLogForNewTeacher.TeacherId = newLesson.TeacherId;
            remindLogForNewTeacher.IsLearner = 0;
            remindLogForNewTeacher.ProcessFlag = 0;
            remindLogForNewTeacher.EmailAt = null;
            remindLogForNewTeacher.RemindTitle = "Lesson Rearrangement Remind";
            remindLogForNewTeacher.ReceivedFlag = 0;
            remindLogForNewTeacher.LessonId = oldLesson.LessonId;
            return remindLogForNewTeacher;
        }

        private string MailContentGenerator(string name, string courseName, string confirmURL, Lesson oldLesson, Lesson newLesson, short userId, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, Pegasus_backend.pegasusContext.Org oldOrg,
            Pegasus_backend.pegasusContext.Org newOrg, Room oldRoom, Room newRoom, DateTime newTodoDate)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>The " +
                    courseName + " lesson given by " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " + 
                    oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName + 
                    " " + oldRoom.RoomName + 
                    " has been rearranged to be given by" + newTeacher.FirstName + " " + newTeacher.LastName + " at " +
                    newOrg.OrgName + " " + newRoom.RoomName + ". Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }
    }
}