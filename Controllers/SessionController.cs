﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Repositories;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : BasicController
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
         private readonly IInvoiceUpdateService _invoiceUpdateService;

        public SessionController(ablemusicContext ablemusicContext, ILogger<SessionController> log, IMapper mapper, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _mapper = mapper;
            _configuration = configuration;
            _invoiceUpdateService = new InvoiceUpdateService(ablemusicContext);
        }

        [CheckModelFilter]
        [HttpPost("{userId}")]
        //补一节课
        public async Task<IActionResult> Post(int userId, [FromBody] LessonViewModel lessonViewModel)
        {
            var result = new Result<Lesson>();
            var lesson = _mapper.Map(lessonViewModel, new Lesson());
            var lessonRemains = new List<LessonRemain>();
            var course = new Course();
            var awaitMakeUpLessons = new List<AwaitMakeUpLesson>();
            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefix").Value;
            try
            {
                lessonRemains = lesson.CourseInstanceId == null ? await _ablemusicContext.LessonRemain.Where(lr => lr.GroupCourseInstanceId == lesson.GroupCourseInstanceId &&
                                                                        lr.LearnerId == lesson.LearnerId).ToListAsync()
                                                                : await _ablemusicContext.LessonRemain.Where(lr => lr.CourseInstanceId == lesson.CourseInstanceId &&
                                                                        lr.LearnerId == lesson.LearnerId).ToListAsync();

                course = lesson.CourseInstanceId == null ? await (from gc in _ablemusicContext.GroupCourseInstance
                                                                  join c in _ablemusicContext.Course on gc.CourseId equals c.CourseId
                                                                  where gc.GroupCourseInstanceId == lesson.GroupCourseInstanceId
                                                                  select new Course
                                                                  {
                                                                      CourseId = c.CourseId,
                                                                      CourseName = c.CourseName,
                                                                      Duration = c.Duration,
                                                                  }).FirstOrDefaultAsync()
                                                          :
                                                            await (from oto in _ablemusicContext.One2oneCourseInstance
                                                                   join c in _ablemusicContext.Course on oto.CourseId equals c.CourseId
                                                                   where oto.CourseInstanceId == lesson.CourseInstanceId
                                                                   select new Course
                                                                   {
                                                                       CourseId = c.CourseId,
                                                                       CourseName = c.CourseName,
                                                                       Duration = c.Duration,
                                                                   }).FirstOrDefaultAsync();

                awaitMakeUpLessons = lesson.CourseInstanceId == null ? await _ablemusicContext.AwaitMakeUpLesson.Where(a => a.IsActive == 1 && a.LearnerId == lesson.LearnerId &&
                                                                             (a.GroupCourseInstanceId.HasValue && lesson.GroupCourseInstanceId == a.GroupCourseInstanceId))
                                                                             .OrderBy(a => a.ExpiredDate).ToListAsync()
                                                                     : await _ablemusicContext.AwaitMakeUpLesson.Where(a => a.IsActive == 1 && a.LearnerId == lesson.LearnerId &&
                                                                             (a.CourseInstanceId.HasValue && lesson.CourseInstanceId == a.CourseInstanceId))
                                                                             .OrderBy(a => a.ExpiredDate).ToListAsync();

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex.Message);
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (lessonRemains.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Remaining Lesson not found";
                return BadRequest(result);
            }
            if (course == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Course not found";
                return BadRequest(result);
            }
            if (awaitMakeUpLessons.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Make Up Lesson not found";
                return BadRequest(result);
            }

            AwaitMakeUpLesson validMakeUpLesson = null;
            TimeSpan duration;
            switch (course.Duration)
            {
                case 1:
                    duration = TimeSpan.FromMinutes(30);
                    break;
                case 2:
                    duration = TimeSpan.FromMinutes(45);
                    break;
                case 3:
                    duration = TimeSpan.FromMinutes(60);
                    break;
                default:
                    duration = TimeSpan.FromMinutes(0);
                    break;
            }
            lesson.EndTime = lesson.BeginTime.Value.Add(duration);

            foreach (var makeUpLesson in awaitMakeUpLessons)
            {
                if ((makeUpLesson.Remaining ?? 0) < GetSplitCount(lesson.BeginTime.Value, lesson.EndTime.Value)) continue;
                if (makeUpLesson.ExpiredDate.Value.Date >= lesson.BeginTime.Value.Date)
                {
                    validMakeUpLesson = makeUpLesson;
                    break;
                }
            }
            if (validMakeUpLesson == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Your make up lesson all expired";
                return BadRequest(result);
            }

            int termId = 0;
            foreach (var lr in lessonRemains)
            {
                if (lr.Quantity > 0)
                {
                    termId = (int)lr.TermId;
                }
            }
            if (termId == 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Lesson Remaining not found";
                return BadRequest(result);
            }

            Teacher teacher = new Teacher();
            var invoice = new Invoice();
            var holidays = new List<Holiday>();
            var org = new pegasusContext.Org();
            var room = new Room();
            var learner = new Learner();
            try
            {
                invoice = lesson.CourseInstanceId == null ? await _ablemusicContext.Invoice.Where(i => i.TermId == termId && i.LearnerId == lesson.LearnerId &&
                                                                  i.GroupCourseInstanceId.HasValue && i.GroupCourseInstanceId == lesson.GroupCourseInstanceId).FirstOrDefaultAsync()
                                                          : await _ablemusicContext.Invoice.Where(i => i.TermId == termId && i.LearnerId == lesson.LearnerId &&
                                                                  i.CourseInstanceId == lesson.CourseInstanceId).FirstOrDefaultAsync();
                teacher = await _ablemusicContext.Teacher.Where(t => t.TeacherId == lesson.TeacherId).FirstOrDefaultAsync();
                org = await _ablemusicContext.Org.Where(o => o.OrgId == lesson.OrgId).FirstOrDefaultAsync();
                room = await _ablemusicContext.Room.Where(r => r.RoomId == lesson.RoomId).FirstOrDefaultAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == lesson.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (learner == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Learner not found";
                return BadRequest(result);
            }
            if (room == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room not found";
                return BadRequest(result);
            }
            if (org == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Org not found";
                return BadRequest(result);
            }
            if (teacher == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Teacher not found";
                return BadRequest(result);
            }
            if (invoice == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Lesson Remain not found";
                return BadRequest(result);
            }

            var lessonConflictCheckerService = new LessonConflictCheckerService(_ablemusicContext, lesson);
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

            DateTime todoDate = lesson.BeginTime.Value.AddDays(-1);
            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }
            }
            DateTime remindScheduleDate = todoDate;

            lesson.LessonId = 0;
            lesson.IsCanceled = 0;
            lesson.Reason = null;
            lesson.CreatedAt = toNZTimezone(DateTime.UtcNow);
            lesson.GroupCourseInstanceId = null;
            lesson.IsTrial = 0;
            lesson.InvoiceNum = invoice.InvoiceNum;
            lesson.IsConfirm = 0;
            lesson.IsPaid = 1;
            lesson.TrialCourseId = null;
            lesson.IsChanged = 0;

            validMakeUpLesson.IsActive = 0;

            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                try
                {
                    await _ablemusicContext.Lesson.AddAsync(lesson);
                    await _ablemusicContext.SaveChangesAsync();
                    validMakeUpLesson.NewLessonId = lesson.LessonId;
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }

                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                todoRepository.AddSingleTodoList("Lesson Rescheduled", TodoListContentGenerator.RearrangedSingleLessonWithoutOldLessonForLearner(
                    learner, lesson, org, room, teacher), (short)userId, todoDate, lesson.LessonId, lesson.LearnerId, null);
                todoRepository.AddSingleTodoList("Lesson Rescheduled", TodoListContentGenerator.RearrangedSingleLessonWithoutOldLessonForTeacher(
                    learner, lesson, org, room, teacher), (short)userId, todoDate, lesson.LessonId, null, teacher.TeacherId);
                var saveTodoResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoResult.IsSuccess)
                {
                    try
                    {
                        _ablemusicContext.Lesson.Remove(lesson);
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = ex.Message + "\n" + saveTodoResult.ErrorMessage;
                        return BadRequest(result);
                    }
                    return BadRequest(saveTodoResult);
                }

                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                remindLogRepository.AddSingleRemindLog(learner.LearnerId, learner.Email, RemindLogContentGenerator.RearrangedSingleLessonWithoutOldLessonForLearner(
                    learner, lesson, org, room, teacher), null, "Lesson Rescheduled", lesson.LessonId, remindScheduleDate);
                remindLogRepository.AddSingleRemindLog(null, teacher.Email, RemindLogContentGenerator.RearrangedSingleLessonWithoutOldLessonForTeacher(
                    learner, lesson, org, room, teacher), teacher.TeacherId, "Lesson Rescheduled", lesson.LessonId, remindScheduleDate);

                var saveRemindResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindResult.IsSuccess)
                {
                    try
                    {
                        _ablemusicContext.Lesson.Remove(lesson);
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = ex.Message + "\n" + saveRemindResult.ErrorMessage;
                        return BadRequest(result);
                    }
                    return BadRequest(saveRemindResult);
                }

                //sending Email
                //List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
                foreach (var todo in saveTodoResult.Data)
                {
                    var remind = saveRemindResult.Data.Find(r => r.LearnerId == todo.LearnerId && r.TeacherId == todo.TeacherId);
                    string currentPersonName;
                    if (todo.TeacherId == null)
                    {
                        currentPersonName = learner.FirstName + " " + learner.LastName;
                    }
                    else
                    {
                        currentPersonName = teacher.FirstName + " " + teacher.LastName;
                    }
                    string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                    string mailContent = EmailContentGenerator.RearrangeLesson(currentPersonName, course.CourseName, lesson, confirmURL, org, room);
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
                dbContextTransaction.Commit();

                //foreach (var mail in notifications)
                //{
                //    _notificationObservable.send(mail);
                //}
            }
            result.Data = lesson;
            result.Data.Learner = null;
            result.Data.Org = null;
            result.Data.Room = null;
            result.Data.Teacher = null;
            return Ok(result);
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
                        .Include(s => s.GroupCourseInstance)
                        .ThenInclude(s => s.Course)
                        .Include(s => s.GroupCourseInstance)
                        .ThenInclude(s => s.Course)
                        .Include(s => s.TrialCourse)
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

                    if (!IsNull(lesson.TrialCourse))
                    {
                        lesson.IsConfirm = 1;
                        lesson.Reason = reason;
                        _ablemusicContext.Update(lesson);
                        await _ablemusicContext.SaveChangesAsync();

                        //fund
                        var TrailCourse = lesson.TrialCourse;
                        var fund1 = _ablemusicContext.Fund.FirstOrDefault(s => s.LearnerId == lesson.LearnerId);
                        fund1.Balance -= TrailCourse.Price;
                        fund1.UpdatedAt = DateTime.Now;
                        _ablemusicContext.Update(fund1);
                        await _ablemusicContext.SaveChangesAsync();

                        //learner transaction
                        var learnerTransaction1 = new LearnerTransaction
                        {
                            LessonId = lesson.LessonId,
                            CreatedAt = toNZTimezone(DateTime.UtcNow).ToShortDateString(),
                            Amount = TrailCourse.Price.ToString(),
                            LearnerId = lesson.LearnerId
                        };
                        _ablemusicContext.Add(learnerTransaction1);
                        await _ablemusicContext.SaveChangesAsync();

                        //teacher transaction
                        var teacherWageRate1 =
                            _ablemusicContext.TeacherWageRates.FirstOrDefault(s =>
                                s.TeacherId == lesson.TeacherId && s.IsActivate == 1);

                        var courseCatogoryId1 = TrailCourse.CourseCategoryId;
                        if (courseCatogoryId1 == 1)
                        {
                            houlyWage = teacherWageRate1.PianoRates;
                        }

                        else if (courseCatogoryId1 == 7)
                        {
                            houlyWage = teacherWageRate1.TheoryRates;
                        }
                        else
                        {
                            houlyWage = teacherWageRate1.OthersRates;
                        }
                        var wageAmout1 = (double)houlyWage * (lesson.EndTime.Value.Subtract(lesson.BeginTime.Value).TotalMinutes / 60);
                        var teacherTransaction1 = new TeacherTransaction
                        {
                            LessonId = lesson.LessonId,
                            CreatedAt = toNZTimezone(DateTime.UtcNow),
                            WageAmount = (decimal)wageAmout1,
                            TeacherId = lesson.TeacherId
                        };
                        _ablemusicContext.Add(teacherTransaction1);
                        await _ablemusicContext.SaveChangesAsync();
                        dbContextTransaction.Commit();
                        result.Data = "success";
                        return Ok(result);

                    }

                    if (!IsNull(lesson.GroupCourseInstanceId))
                    {
                        lesson.IsConfirm = 1;
                        lesson.Reason = reason;
                        _ablemusicContext.Update(lesson);
                        await _ablemusicContext.SaveChangesAsync();

                        var houlywage = _ablemusicContext.TeacherWageRates
                            .FirstOrDefault(s => s.TeacherId == lesson.TeacherId && s.IsActivate == 1).GroupRates;
                        var GroupWageAmout = (double)houlywage * (lesson.EndTime.Value.Subtract(lesson.BeginTime.Value).TotalMinutes / 60);
                        var teacherTransactionForGroup = new TeacherTransaction
                        {
                            LessonId = lesson.LessonId,
                            CreatedAt = toNZTimezone(DateTime.UtcNow),
                            WageAmount = (decimal)GroupWageAmout,
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
                    LessonRemain lessonRemain;
                    lessonRemain = _ablemusicContext.LessonRemain.FirstOrDefault(s =>
                        s.LearnerId == lesson.LearnerId && s.CourseInstanceId == lesson.CourseInstanceId);

                    //
                    if (lessonRemain == null)
                    {
                        var invoice = _ablemusicContext.InvoiceWaitingConfirm.FirstOrDefault(s => s.InvoiceNum == lesson.InvoiceNum && s.IsActivate == 1);
                        var newlessonRemain = new LessonRemain
                        {
                            Quantity = 0,
                            TermId = invoice.TermId,
                            ExpiryDate = invoice.EndDate.Value.AddMonths(3),
                            CourseInstanceId = lesson.CourseInstanceId,
                            LearnerId = lesson.LearnerId
                        };
                        _ablemusicContext.Add(newlessonRemain);
                        await _ablemusicContext.SaveChangesAsync();
                    }

                    lessonRemain = _ablemusicContext.LessonRemain.FirstOrDefault(s =>
                        s.LearnerId == lesson.LearnerId && s.CourseInstanceId == lesson.CourseInstanceId);
                    //

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
                        LessonId = lesson.LessonId,
                        CreatedAt = toNZTimezone(DateTime.UtcNow).ToShortDateString(),
                        Amount = course.Price.ToString(),
                        LearnerId = lesson.LearnerId
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
                    var wageAmout = (double)houlyWage * (lesson.EndTime.Value.Subtract(lesson.BeginTime.Value).TotalMinutes / 60);
                    var teacherTransaction = new TeacherTransaction
                    {
                        LessonId = lesson.LessonId,
                        CreatedAt = toNZTimezone(DateTime.UtcNow),
                        WageAmount = (decimal)wageAmout,
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
            catch (Exception ex)
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
            todoList.ProcessedAt = toNZTimezone(DateTime.UtcNow);
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
        [Route("MakeUpSplitLesson/{lessonId}/{isAfter}/{staffId}")]
        [HttpPut]
        //补15分钟课
        public async Task<IActionResult> MakeUpSplitLesson(int lessonId, short isAfter, short staffId)
        {
            var result = new Result<string>();
            //AwaitMakeUpLesson makeUpLesson;
            try
            {
                var nowDate = toNZTimezone(DateTime.UtcNow);
                var lesson = await _ablemusicContext.Lesson.
                    Where(l => l.LessonId == lessonId && l.IsCanceled != 1).FirstOrDefaultAsync();
                var splittedLesson = new SplittedLesson();

                if (lesson == null) throw new Exception("You selected a wrong lesson to make up!");
                if (lesson.CourseInstanceId == null) throw new Exception("Trial lesson and group lesson can not be made up!");
                int CourseInstanceId = lesson.CourseInstanceId.Value;
                var makeUpLesson = await _ablemusicContext.AwaitMakeUpLesson.
                Where(r => r.CourseInstanceId == CourseInstanceId &&
                     r.ExpiredDate > nowDate && r.IsActive == 1 && r.Remaining > 0)
                     .OrderBy(r => r.ExpiredDate).FirstOrDefaultAsync();

                if (makeUpLesson == null) throw new Exception("No make up lesson!");

                if (isAfter == 1)
                    lesson.EndTime = lesson.EndTime.Value.AddMinutes(15);
                else
                    lesson.BeginTime = lesson.BeginTime.Value.AddMinutes(-15);

                makeUpLesson.Remaining = (byte)(makeUpLesson.Remaining - 1);
                if (makeUpLesson.Remaining == 0) makeUpLesson.IsActive = 0;

                splittedLesson.AwaitId = makeUpLesson.AwaitId;
                splittedLesson.LessonId = lesson.LessonId;
                splittedLesson.IsAfter = isAfter;
                splittedLesson.CreatedAt = nowDate;
                splittedLesson.StaffId = staffId;

                await _ablemusicContext.AddAsync(splittedLesson);
                _ablemusicContext.Update(lesson);
                _ablemusicContext.Update(makeUpLesson);

                var lessonConflictCheckerService = new LessonConflictCheckerService(_ablemusicContext, lesson.BeginTime.Value,
                lesson.EndTime.Value, lesson.RoomId.Value, lesson.OrgId, (int)lesson.TeacherId, lesson.LessonId);
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
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }
            return Ok(result);  //Should redirect to a success page!
        }
        [HttpGet("[action]/{learnerId}")]
        //public async Task<IActionResult> GetLessonsBetweenDate(DateTime? beginDate, DateTime? endDate, int? userId)
        public async Task<IActionResult> GetMakeupSessions(int learnerId)
        //public async Task<IActionResult> CancelConfirm2(int remindId)

        {
            var result = new Result<IEnumerable<object>>();
            try
            {
                result.Data = await _ablemusicContext.AwaitMakeUpLesson.
                    Include(a => a.CourseInstance).ThenInclude(ci => ci.Course).
                    Include(a => a.MissedLesson).
                    Include(a => a.NewLesson).
                    Include(a => a.SplittedLesson).
                    ThenInclude(sp => sp.Lesson).
                    Where(a => a.LearnerId == learnerId).Select(a => new
                    {
                        awaitId = a.AwaitId,
                        CreateAt = a.CreateAt,
                        SchduledAt = a.SchduledAt,
                        ExpiredDate = a.ExpiredDate,
                        MissedLessonId = a.MissedLessonId,
                        NewLessonId = a.NewLessonId,
                        IsActive = a.IsActive,
                        Remaining = a.Remaining,
                        CourseInstanceId = a.CourseInstanceId,
                        CourseInstance = new { CourseId = a.CourseInstance.CourseId, CourseName = a.CourseInstance.Course.CourseName },
                        MissedLesson = new { Org = a.MissedLesson.Org.Abbr, Teacher = a.MissedLesson.Teacher.FirstName, beginDate = a.MissedLesson.BeginTime },
                        NewLesson = new { Org = a.NewLesson.Org.Abbr, Teacher = a.NewLesson.Teacher.FirstName, beginDate = a.NewLesson.BeginTime },
                        SplittedLesson = a.SplittedLesson.Select(p => new { p.Lesson.BeginTime, p.Lesson.IsCanceled, p.Lesson.IsConfirm })
                    })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("[action]/{learnerId}/{courseId}")]
        //public async Task<IActionResult> GetLessonsBetweenDate(DateTime? beginDate, DateTime? endDate, int? userId)
        public async Task<IActionResult> GetRemainingAmount(int learnerId, int courseId)
        //public async Task<IActionResult> CancelConfirm2(int remindId)

        {
            var result = new Result<IEnumerable<object>>();
            try
            {
                result.Data = await _ablemusicContext.AwaitMakeUpLesson
                    .Include(a => a.CourseInstance).ThenInclude(c => c.Course.Duration)
                    .Where(a => a.LearnerId == learnerId
                        && a.CourseInstance.CourseId == courseId
                        && a.IsActive == 1 && a.Remaining > 0
                    ).Select(a => new
                    {
                        remaining = a.Remaining,
                        duration = a.CourseInstance.Course.Duration
                    })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("[action]/{instanceId}")]
        //public async Task<IActionResult> GetLessonsBetweenDate(DateTime? beginDate, DateTime? endDate, int? userId)
        public async Task<IActionResult> GetGroupMakeupSessions(int instanceId)
        //public async Task<IActionResult> CancelConfirm2(int remindId)

        {
            var result = new Result<IEnumerable<object>>();
            try
            {
                result.Data = await _ablemusicContext.AwaitMakeUpLesson.
                    Include(a => a.GroupCourseInstance).ThenInclude(ci => ci.Course).
                    Include(a => a.MissedLesson).
                    Include(a => a.NewLesson).
                    Include(a => a.SplittedLesson).
                    ThenInclude(sp => sp.Lesson).
                    Where(a => a.GroupCourseInstanceId == instanceId).Select(a => new
                    {
                        awaitId = a.AwaitId,
                        CreateAt = a.CreateAt,
                        SchduledAt = a.SchduledAt,
                        ExpiredDate = a.ExpiredDate,
                        MissedLessonId = a.MissedLessonId,
                        NewLessonId = a.NewLessonId,
                        IsActive = a.IsActive,
                        Remaining = a.Remaining,
                        CourseInstance = new { CourseId = a.GroupCourseInstance.CourseId, CourseName = a.GroupCourseInstance.Course.CourseName },
                        MissedLesson = new
                        {
                            Org = a.MissedLesson.Org.Abbr,
                            Teacher = a.MissedLesson.Teacher.FirstName,
                            BeginTime = a.MissedLesson.BeginTime,
                            EndTime = a.MissedLesson.EndTime,
                            CourseName = a.GroupCourseInstance.Course.CourseName,
                            CourseId = a.CourseInstance.CourseId,
                            awaitId = a.AwaitId,
                            TeacherId = a.MissedLesson.Teacher.TeacherId,
                            RoomId = a.MissedLesson.RoomId,
                            OrgId = a.MissedLesson.OrgId
                        },
                        NewLesson = new { Org = a.NewLesson.Org.Abbr, Teacher = a.NewLesson.Teacher.FirstName, beginDate = a.NewLesson.BeginTime },
                        SplittedLesson = a.SplittedLesson.Select(p => new { p.Lesson.BeginTime, p.Lesson.IsCanceled, p.Lesson.IsConfirm })
                    })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> CancelSessons([FromBody] CancelSessionsViewModel cancelSessions)
        {
            Result<string> result = new Result<string>();
            string reason = cancelSessions.Reason;
            short userId = cancelSessions.UserId;
            Boolean isInvoiceChange = cancelSessions.IsInvoiceChange;
            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var lessonId in cancelSessions.LessonIds)
                    {
                        result = await CancelSesson(lessonId, reason, userId,isInvoiceChange);
                        if (result.IsSuccess == false)
                        {
                            dbContextTransaction.Rollback();
                            throw new Exception(result.ErrorMessage);
                        }
                    }
                    if (cancelSessions.IsInvoiceChange == true)
                        _invoiceUpdateService.InvoiceUpdate(cancelSessions.LessonIds);                    
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
            }

            return Ok(result);
        }
        // PUT: api/Session/5
        [HttpPut("{sessionId}/{reason}/{userId}")]
        public async Task<IActionResult> PutSesson(int sessionId, string reason, short userId)
        {
            // var result = new Result<string>();
            Result<string> result = new Result<string>();
            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {

                try
                {
                    result = await CancelSesson(sessionId, reason, userId,false);
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
            }

            return Ok(result);
        }

        private async Task<Result<String>> CancelSesson(int sessionId, string reason, short userId,bool isInvoiceChange)
        {
            var result = new Result<string>();
            var awaitMakeUpLesson = new AwaitMakeUpLesson();
            Lesson lesson;
            try
            {
                lesson = await _ablemusicContext.Lesson.Where(x => x.LessonId == sessionId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return result;
            }

            if (lesson == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Session not found";
                return result;
            }
            if (lesson.IsCanceled == 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "This lesson has already been cancelled";
                return result;
            }
            if (string.IsNullOrEmpty(reason))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Reason is required";
                return result;
            }
            var user = await _ablemusicContext.User.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User id not found";
                return result;
            }

            lesson.IsCanceled = 1;
            lesson.Reason = reason;
            //if don't change invoice, should save  makeup lessones
            if (isInvoiceChange==false){
                awaitMakeUpLesson.MissedLessonId = lesson.LessonId;
                awaitMakeUpLesson.LearnerId = lesson.LearnerId;
                awaitMakeUpLesson.CourseInstanceId = lesson.CourseInstanceId;
                awaitMakeUpLesson.GroupCourseInstanceId = lesson.GroupCourseInstanceId;
                awaitMakeUpLesson.CreateAt = toNZTimezone(DateTime.UtcNow);
                awaitMakeUpLesson.IsActive = 1;
                awaitMakeUpLesson.ExpiredDate = lesson.BeginTime.Value.Date.AddMonths(3);
                awaitMakeUpLesson.Remaining = GetSplitCount(lesson.BeginTime.Value, lesson.EndTime.Value);
                await _ablemusicContext.AwaitMakeUpLesson.AddAsync(awaitMakeUpLesson);
            }

            bool isGroupCourse = lesson.LearnerId == null;
            Teacher teacher;
            Course course;
            List<Holiday> holidays;
            try
            {
                teacher = await _ablemusicContext.Teacher.FirstOrDefaultAsync(t => t.TeacherId == lesson.TeacherId);
                if (lesson.IsTrial == 1)
                {
                    course = await _ablemusicContext.Course.Where(c => c.CourseId == lesson.TrialCourseId).FirstOrDefaultAsync();
                }
                else
                {
                    course = isGroupCourse ? await (from c in _ablemusicContext.Course
                                                    join gc in _ablemusicContext.GroupCourseInstance on c.CourseId equals gc.CourseId
                                                    where gc.GroupCourseInstanceId == lesson.GroupCourseInstanceId
                                                    select new Course
                                                    {
                                                        CourseId = c.CourseId,
                                                        CourseName = c.CourseName
                                                    }).FirstOrDefaultAsync() :
                                          await (from c in _ablemusicContext.Course
                                                 join oto in _ablemusicContext.One2oneCourseInstance on c.CourseId equals oto.CourseId
                                                 where oto.CourseInstanceId == lesson.CourseInstanceId
                                                 select new Course
                                                 {
                                                     CourseId = c.CourseId,
                                                     CourseName = c.CourseName
                                                 }).FirstOrDefaultAsync();
                }
                holidays = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
            }

            if (course == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Fail to found course name under this lesson";
                return result;
            }
            string courseName = course.CourseName;

            DateTime todoDate = lesson.BeginTime.Value.AddDays(-1);
            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }
            }

            DateTime remindScheduleDate = todoDate;

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
                    return result;
                }

                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                todoRepository.AddSingleTodoList("Cancellation to Remind", TodoListContentGenerator.CancelSingleLessonForTeacher(teacher,
                    lesson, reason), userId, todoDate, lesson.LessonId, null, teacher.TeacherId);
                todoRepository.AddSingleTodoList("Cancellation to Remind", TodoListContentGenerator.CancelSingleLessonForLearner(learner,
                    lesson, reason), userId, todoDate, lesson.LessonId, learner.LearnerId, null);
                var saveTodoResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = saveTodoResult.ErrorMessage;
                    return result;
                }
                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                remindLogRepository.AddSingleRemindLog(null, teacher.Email, RemindLogContentGenerator.CancelSingleLessonForTeacher(courseName,
                    lesson, reason), teacher.TeacherId, "Lesson Cancellation Remind", lesson.LessonId, remindScheduleDate);
                remindLogRepository.AddSingleRemindLog(learner.LearnerId, learner.Email, RemindLogContentGenerator.CancelSingleLessonForLearner(courseName,
                    lesson, reason), null, "Lesson Cancellation Remind", lesson.LessonId, remindScheduleDate);
                var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindLogResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = saveRemindLogResult.ErrorMessage;
                    return result;
                }
                try
                {
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    result.ErrorMessage = e.Message;
                    result.IsSuccess = false;
                    return result;
                }

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
                        currentPersonName = teacher.FirstName + " " + teacher.LastName;
                    }
                    string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                    string mailContent = EmailContentGenerator.CancelLesson(currentPersonName, courseName, lesson, reason, confirmURL);
                    remindLogRepository.UpdateContent(remind.RemindId, mailContent);
                    //notifications.Add(new NotificationEventArgs(remind.Email, "Lesson Cancellation Confirm", mailContent, remind.RemindId));
                }
                var remindLogUpdateContentResult = await remindLogRepository.SaveUpdatedContentAsync();
                if (!remindLogUpdateContentResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = remindLogUpdateContentResult.ErrorMessage;
                    return result;
                }
                //foreach (var mail in notifications)
                //{
                //    _notificationObservable.send(mail);
                //}
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
                    return result;
                }

                var todoForLearnersIdMapContent = new Dictionary<int, string>();
                var remindLogForLearnersMapContent = new Dictionary<Learner, string>();
                foreach (var l in learners)
                {
                    todoForLearnersIdMapContent.Add(l.LearnerId, TodoListContentGenerator.CancelSingleLessonForLearner(l, lesson, reason));
                    remindLogForLearnersMapContent.Add(l, RemindLogContentGenerator.CancelSingleLessonForLearner(courseName, lesson, reason));
                }

                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                todoRepository.AddSingleTodoList("Cancellation to Remind", TodoListContentGenerator.CancelSingleLessonForTeacher(teacher,
                    lesson, reason), userId, todoDate, lesson.LessonId, null, teacher.TeacherId);
                todoRepository.AddMutipleTodoLists("Cancellation to Remind", todoForLearnersIdMapContent, userId, todoDate, lesson.LessonId, null);
                var saveTodoListsResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoListsResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = saveTodoListsResult.ErrorMessage;
                    return result;
                }


                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                remindLogRepository.AddSingleRemindLog(null, teacher.Email, RemindLogContentGenerator.CancelSingleLessonForTeacher(courseName,
                    lesson, reason), teacher.TeacherId, "Lesson Cancellation Remind", lesson.LessonId, remindScheduleDate);
                remindLogRepository.AddMultipleRemindLogs(remindLogForLearnersMapContent, null, "Lesson Cancellation Remind", lesson.LessonId, remindScheduleDate);
                var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindLogResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = saveRemindLogResult.ErrorMessage;
                    return result;
                }

                try
                {
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    result.ErrorMessage = e.Message;
                    result.IsSuccess = false;
                    return result;
                }

                //sending Email
                //List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
                foreach (var todo in saveTodoListsResult.Data)
                {
                    var remind = saveRemindLogResult.Data.Find(r => r.LearnerId == todo.LearnerId && r.TeacherId == todo.TeacherId);
                    string currentPersonName;
                    if (todo.TeacherId == null)
                    {
                        currentPersonName = learners.Find(l => l.LearnerId == todo.LearnerId).FirstName + " " + learners.Find(l => l.LearnerId == todo.LearnerId).LastName;
                    }
                    else
                    {
                        currentPersonName = teacher.FirstName + " " + teacher.LastName;
                    }
                    string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                    string mailContent = EmailContentGenerator.CancelLesson(currentPersonName, courseName, lesson, reason, confirmURL);
                    remindLogRepository.UpdateContent(remind.RemindId, mailContent);
                    //notifications.Add(new NotificationEventArgs(remind.Email, "Lesson Cancellation Confirm", mailContent, remind.RemindId));
                }
                var remindLogUpdateContentResult = await remindLogRepository.SaveUpdatedContentAsync();
                if (!remindLogUpdateContentResult.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = remindLogUpdateContentResult.ErrorMessage;
                    return result;
                }
                //foreach (var mail in notifications)
                //{
                //    _notificationObservable.send(mail);
                //}

            }
            return result;
        }

        private bool LessonExists(int id)
        {
            return _ablemusicContext.Lesson.Any(e => e.LessonId == id);
        }     
    }
}