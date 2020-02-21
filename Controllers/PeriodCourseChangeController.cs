using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Services;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Repositories;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodCourseChangeController : BasicController
    {
        private readonly IConfiguration _configuration;

        public PeriodCourseChangeController(ablemusicContext ablemusicContext, ILogger<PeriodCourseChangeController> log, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _configuration = configuration;
        }

        // POST: api/PeriodCourseChange
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostPeriodCourseChange([FromBody] PeriodCourseChangeViewModel inputObj)
        {
            var result = new Result<List<object>>();
            List<Lesson> exsitingLessons;
            Learner learner;
            List<Holiday> holidays;
            List<Amendment> exsitingAmendments;
            Amendment amendment = new Amendment();
            DateTime? todoDateEnd = null;
            DateTime todoDateBegin = inputObj.BeginDate;
            List<Term> terms;
            dynamic courseInfo;
            dynamic newCourseInfo;
            AvailableDays availableDay;

            try
            {
                if (inputObj.EndDate.HasValue)
                {
                    exsitingLessons = await _ablemusicContext.Lesson.Where(l => l.LearnerId == inputObj.LearnerId && l.IsCanceled != 1 &&
                    l.BeginTime.Value.Date <= inputObj.EndDate.Value.Date && l.BeginTime.Value.Date >= inputObj.BeginDate.Date&&
                    l.CourseInstanceId == inputObj.InstanceId).ToListAsync();
                } else
                {
                    exsitingLessons = await _ablemusicContext.Lesson.Where(l => l.LearnerId == inputObj.LearnerId && l.IsCanceled != 1 &&
                    l.BeginTime.Value.Date >= inputObj.BeginDate.Date &&
                    l.CourseInstanceId == inputObj.InstanceId).ToListAsync();
                }
                terms = await _ablemusicContext.Term.ToListAsync();
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
                availableDay = await _ablemusicContext.AvailableDays.Where(ad => ad.TeacherId == inputObj.TeacherId && ad.DayOfWeek == inputObj.DayOfWeek && ad.OrgId == inputObj.OrgId).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(courseInfo == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Course schedule or course instance not found";
                return BadRequest(result);
            }
            if(availableDay == null || availableDay.RoomId == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room Id not found";
                return BadRequest(result);
            }
            
            try
            {
                newCourseInfo = new
                {
                    newOrg = await _ablemusicContext.Org.Where(o => o.OrgId == inputObj.OrgId).FirstOrDefaultAsync(),
                    newRoom = await _ablemusicContext.Room.Where(r => r.RoomId == availableDay.RoomId).FirstOrDefaultAsync(),
                };
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == inputObj.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
                exsitingAmendments = await _ablemusicContext.Amendment.Where(a => a.LearnerId == inputObj.LearnerId && a.AmendType == 2 &&
                a.BeginDate == inputObj.BeginDate && a.EndDate == inputObj.EndDate && inputObj.OrgId == a.OrgId &&
                inputObj.BeginTime == a.BeginTime && inputObj.DayOfWeek == a.DayOfWeek && inputObj.InstanceId == a.CourseInstanceId &&
                availableDay.RoomId == a.RoomId && inputObj.IsTemporary == a.IsTemporary
                ).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(newCourseInfo == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Org id or room id not found";
                return BadRequest(result);
            }
            if (exsitingAmendments.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "The same change has already being made";
                return BadRequest(result);
            }
            if (!inputObj.EndDate.HasValue && inputObj.IsTemporary == 1)
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
           
            foreach (var lesson in exsitingLessons.Reverse<Lesson>())
            {
                // if (courseInfo.DayOfWeek != lesson.BeginTime.Value.ToDayOfWeek() || lesson.IsChanged == 1 ||
                //     lesson.BeginTime.Value.TimeOfDay != courseInfo.BeginTime)
                if ( lesson.IsChanged == 1||lesson.IsChanged == 2 )
                {
                    exsitingLessons.Remove(lesson);
                }
            }

            var newLessonsMapOldLesson = new Dictionary<Lesson,Lesson>();

            foreach (var lesson in exsitingLessons)
            {
                DateTime beginTime = lesson.BeginTime.Value;
                DateTime endTime = lesson.EndTime.Value;
                // if (inputObj.DayOfWeek != courseInfo.DayOfWeek)
                // {
                    beginTime = beginTime.SetDateByDayOfWeekInWeek(inputObj.DayOfWeek);
                    endTime = endTime.SetDateByDayOfWeekInWeek(inputObj.DayOfWeek);
                // }
                beginTime = beginTime.Date + inputObj.BeginTime;
                endTime = endTime.Date + inputObj.EndTime;
                newLessonsMapOldLesson.Add(new Lesson
                {
                    LearnerId = lesson.LearnerId,
                    RoomId = availableDay.RoomId,
                    TeacherId = inputObj.TeacherId,
                    OrgId = Convert.ToInt16(inputObj.OrgId),
                    IsCanceled = 0,
                    Reason = null,
                    CreatedAt = DateTime.UtcNow.ToNZTimezone(),
                    CourseInstanceId = lesson.CourseInstanceId,
                    GroupCourseInstanceId = lesson.GroupCourseInstanceId,
                    IsTrial = lesson.IsTrial,
                    BeginTime = beginTime,
                    EndTime = endTime,
                    InvoiceNum = lesson.InvoiceNum,
                    IsConfirm = lesson.IsConfirm,
                    TrialCourseId = lesson.TrialCourseId,
                    IsChanged = lesson.IsChanged,
                    IsPaid = lesson.IsPaid,
                    NewLessonId = lesson.NewLessonId,
                }, lesson);
                lesson.IsCanceled = 1;
            }

            amendment.CourseInstanceId = inputObj.InstanceId;
            amendment.OrgId = Convert.ToInt16(inputObj.OrgId);
            amendment.DayOfWeek = inputObj.DayOfWeek;
            amendment.BeginTime = inputObj.BeginTime;
            amendment.EndTime = inputObj.EndTime;
            amendment.LearnerId = inputObj.LearnerId;
            amendment.RoomId = availableDay.RoomId;
            amendment.BeginDate = inputObj.BeginDate;
            amendment.EndDate = inputObj.IsTemporary == 1 ? inputObj.EndDate : null;
            amendment.CreatedAt = toNZTimezone(DateTime.UtcNow);
            amendment.Reason = inputObj.Reason;
            amendment.IsTemporary = inputObj.IsTemporary;
            amendment.AmendType = 2;
            amendment.TeacherId = inputObj.TeacherId;
            amendment.CourseScheduleId = inputObj.CourseScheduleId;

            DateTime endDate;
            
            if(inputObj.IsTemporary == 1)
            {
                endDate = inputObj.EndDate.Value;
            }
            else
            {
                Term currentTerm = null;
                Term nextTerm = null;
                DateTime lastDateInAllTerms = inputObj.BeginDate;
                foreach (var t in terms)
                {
                    lastDateInAllTerms = lastDateInAllTerms >= t.EndDate.Value ? lastDateInAllTerms : t.EndDate.Value;
                    if(amendment.BeginDate.Value > t.BeginDate.Value && amendment.BeginDate.Value < t.EndDate.Value)
                    {
                        currentTerm = t;
                    }
                }
                if (currentTerm == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Term not found";
                    return BadRequest(result);
                }
                DateTime currentDay = currentTerm.EndDate.Value.AddDays(1);
                while (currentDay < lastDateInAllTerms)
                {
                    nextTerm = terms.Find(t => t.BeginDate.Value < currentDay && t.EndDate.Value > currentDay);
                    if (nextTerm != null)
                    {
                        break;
                    }
                    else
                    {
                        currentDay = currentDay.AddDays(1);
                    }
                }
                if(nextTerm == null)
                {
                    endDate = currentTerm.EndDate.Value;
                }
                else
                {
                    endDate = nextTerm.EndDate.Value;
                }
            }
            
            var visualLessonsForCheckingConflict = new List<Lesson>();

            DateTime currentDate = amendment.BeginDate.Value;
            int currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
            while (currentDayOfWeek != amendment.DayOfWeek)
            {
                currentDate = currentDate.AddDays(1);
                currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
            }
            while (currentDate <= endDate)
            {
                visualLessonsForCheckingConflict.Add(new Lesson
                {
                    RoomId = amendment.RoomId,
                    TeacherId = amendment.TeacherId,
                    OrgId = amendment.OrgId.Value,
                    BeginTime = currentDate.Add(amendment.BeginTime.Value),
                    EndTime = currentDate.Add(amendment.EndTime.Value),
                });
                currentDate = currentDate.AddDays(7);
                currentDayOfWeek = currentDate.DayOfWeek == 0 ? 7 : (int)currentDate.DayOfWeek;
            }

            var lessonConflictCheckerService = new LessonConflictCheckerService(_ablemusicContext, inputObj.BeginDate, endDate);
            try
            {
                await lessonConflictCheckerService.LoadAllProtentialConflictLessonsToMemoryAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            Dictionary<Lesson, object> lessonIdMapConflictCheckResult = new Dictionary<Lesson, object>();
            foreach (var lesson in visualLessonsForCheckingConflict)
            {
                lessonConflictCheckerService.ConfigureLessonToCheck(lesson);
                Result<List<object>> lessonConflictCheckResult = lessonConflictCheckerService.CheckBothRoomAndTeacherInMemory();
                if (!lessonConflictCheckResult.IsSuccess)
                {
                    lessonIdMapConflictCheckResult.Add(lesson, lessonConflictCheckResult);
                }
            }

            if(lessonIdMapConflictCheckResult.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Conflict Found";
                result.Data = new List<object>
                {
                    new { ArrangedLessonFound = exsitingLessons.Count }
                };
                // foreach(var conflictResult in lessonIdMapConflictCheckResult)
                // {
                //     result.Data.Add(new
                //     {
                //         LessonWithConflict = conflictResult.Key,
                //         ConflictDetail = conflictResult.Value
                //     });
                // }
                return BadRequest(result);
            }

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

            DateTime remindScheduleDateBegin = todoDateBegin;

            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                todoRepository.AddSingleTodoList("Course Schedule Changing Reminder", TodoListContentGenerator.PeriodCourseChangeForLearner(courseInfo,
                    newCourseInfo, inputObj, todoDateBegin), inputObj.UserId, todoDateBegin, null, inputObj.LearnerId, null);
                todoRepository.AddSingleTodoList("Course Schedule Changing Reminder", TodoListContentGenerator.PeriodCourseChangeForTeacher(courseInfo,
                    newCourseInfo, inputObj, todoDateBegin), inputObj.UserId, todoDateBegin, null, null, courseInfo.TeacherId);
                if (inputObj.EndDate.HasValue && inputObj.IsTemporary == 1)
                {
                    todoRepository.AddSingleTodoList("Course schedule Changing Reminder", TodoListContentGenerator.PeriodCourseChangeForLearner(courseInfo,
                        newCourseInfo, inputObj, todoDateEnd.Value), inputObj.UserId, todoDateEnd.Value, null, inputObj.LearnerId, null);
                    todoRepository.AddSingleTodoList("Course schedule Changing Reminder", TodoListContentGenerator.PeriodCourseChangeForTeacher(courseInfo,
                        newCourseInfo, inputObj, todoDateEnd.Value), inputObj.UserId, todoDateEnd.Value, null, null, courseInfo.TeacherId);
                }
                var saveTodoResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoResult.IsSuccess)
                {
                    return BadRequest(saveTodoResult);
                }

                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                remindLogRepository.AddSingleRemindLog(courseInfo.LearnerId, courseInfo.LearnerEmail, RemindLogContentGenerator.PeriodCourseChangeForLearner(
                    courseInfo, newCourseInfo, inputObj), null, "Course schedule Changing Reminder", null, remindScheduleDateBegin);
                remindLogRepository.AddSingleRemindLog(null, courseInfo.TeacherEmail, RemindLogContentGenerator.PeriodCourseChangeForTeacher(courseInfo,
                    newCourseInfo, inputObj), courseInfo.TeacherId, "Course schedule Changing Reminder", null, remindScheduleDateBegin);

                var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindLogResult.IsSuccess)
                {
                    return BadRequest(saveRemindLogResult);
                }

                try
                {
                    foreach (var lesson in newLessonsMapOldLesson)
                    {
                        await _ablemusicContext.Lesson.AddAsync(lesson.Key);
                    }
                    await _ablemusicContext.Amendment.AddAsync(amendment);
                    await _ablemusicContext.SaveChangesAsync();
                    foreach (var lesson in exsitingLessons)
                    {
                        foreach (var m in newLessonsMapOldLesson)
                        {
                            if (m.Value.LessonId == lesson.LessonId)
                            {
                                lesson.NewLessonId = m.Key.LessonId;
                            }
                        }
                    }
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }

                //sending Email
                //List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
                foreach (var remind in saveRemindLogResult.Data)
                {
                    string currentPersonName;
                    if (remind.TeacherId == null)
                    {
                        currentPersonName = courseInfo.LearnerFirstName + " " + courseInfo.LearnerLastName;
                    }
                    else
                    {
                        currentPersonName = courseInfo.TeacherFirstName + " " + courseInfo.TeacherLastName;
                    }
                    string mailContent = EmailContentGenerator.PeriodCourseChange(currentPersonName, courseInfo, newCourseInfo, inputObj);
                    remindLogRepository.UpdateContent(remind.RemindId, mailContent);
                    //notifications.Add(new NotificationEventArgs(remind.Email, "Course schedule Changing Reminder", mailContent, remind.RemindId));
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

            result.Data = new List<object>
            {
                new { ArrangedLessonFound = exsitingLessons.Count },
                new { ArrangedLessonChanged = newLessonsMapOldLesson.Count }
            };

            return Ok(result);
        }
    }
}
