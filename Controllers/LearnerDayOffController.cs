using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Repositories;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerDayOffController : BasicController
    {
        private readonly IConfiguration _configuration;

        public LearnerDayOffController(ablemusicContext ablemusicContext, ILogger<LearnerDayOffController> log, IConfiguration configuration) : base(ablemusicContext, log)
        {
            _configuration = configuration;
        }

        // POST: api/LearnerDayOff
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostDayoff([FromBody] LearnerDayoffViewModel inputObj)
        {
            var result = new Result<object>();
            List<Lesson> lessons;
            List<Amendment> amendments = new List<Amendment>();
            List<Amendment> exsitsAmendment;
            Learner learner;
            dynamic courseSchedules;
            List<Holiday> holidays;
            try
            {
                lessons = await _ablemusicContext.Lesson.Where(l => l.LearnerId == inputObj.LearnerId && l.IsCanceled != 1 && l.CourseInstanceId.HasValue && inputObj.InstanceIds.Contains(l.CourseInstanceId.Value) &&
                l.BeginTime.Value.Date >= inputObj.BeginDate.Date && l.BeginTime.Value.Date <= inputObj.EndDate.Date).OrderBy(l => l.CourseInstanceId).ToListAsync();
                courseSchedules = await (from i in _ablemusicContext.One2oneCourseInstance
                                         join cs in _ablemusicContext.CourseSchedule on i.CourseInstanceId equals cs.CourseInstanceId
                                         join l in _ablemusicContext.Learner on i.LearnerId equals l.LearnerId
                                         join t in _ablemusicContext.Teacher on i.TeacherId equals t.TeacherId
                                         join c in _ablemusicContext.Course on i.CourseId equals c.CourseId
                                         join o in _ablemusicContext.Org on i.OrgId equals o.OrgId
                                         join r in _ablemusicContext.Room on i.RoomId equals r.RoomId
                                         where inputObj.InstanceIds.Contains(cs.CourseInstanceId.Value)
                                         select new
                                         {
                                             cs.CourseScheduleId,
                                             cs.CourseInstanceId,
                                             i.OrgId,
                                             o.OrgName,
                                             i.RoomId,
                                             r.RoomName,
                                             i.CourseId,
                                             c.CourseName,
                                             i.TeacherId,
                                             TeacherFirstName = t.FirstName,
                                             TeacherLastName = t.LastName,
                                             TeacherEmail = t.Email,
                                             cs.DayOfWeek,
                                             i.LearnerId,
                                             LearnerFirstName = l.FirstName,
                                             LearnerLastName = l.LastName,
                                             LearnerEmail = l.Email,
                                         }).ToListAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == inputObj.LearnerId).FirstOrDefaultAsync();
                holidays = await _ablemusicContext.Holiday.ToListAsync();
                exsitsAmendment = await _ablemusicContext.Amendment.Where(a => a.LearnerId == inputObj.LearnerId && a.AmendType == 1 &&
                a.CourseInstanceId.HasValue && inputObj.InstanceIds.Contains(a.CourseInstanceId.Value) &&
                ((inputObj.BeginDate >= a.BeginDate && inputObj.BeginDate <= a.EndDate) ||
                (inputObj.EndDate >= a.BeginDate && inputObj.EndDate <= a.EndDate) ||
                (inputObj.BeginDate <= a.BeginDate && inputObj.EndDate >= a.EndDate))).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if(courseSchedules.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Course schedule not found";
                return BadRequest(result);
            }
            if(learner == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Learner not found";
                return BadRequest(result);
            }
            if(exsitsAmendment.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "There is a conflict of date on your previous dayoff";
                return BadRequest(result);
            }
            var remaining = GetSplitCount(lessons[0].BeginTime.Value,lessons[0].EndTime.Value);
            Dictionary<string, int> invoiceNumsMapLessonQuantity = new Dictionary<string, int>();
            foreach(var lesson in lessons)
            {
               
                if (inputObj.IsInvoiceChange && lesson.InvoiceNum != null)
                {
                    if (!invoiceNumsMapLessonQuantity.ContainsKey(lesson.InvoiceNum))
                    {
                        invoiceNumsMapLessonQuantity.Add(lesson.InvoiceNum, 1);
                    }
                    else
                    {
                        invoiceNumsMapLessonQuantity[lesson.InvoiceNum]++;
                    }
                }
                lesson.IsCanceled = 1;
                lesson.Reason = inputObj.Reason;
            }

            var invoices = new List<Invoice>();
            var invoiceWaitingConfirms = new List<InvoiceWaitingConfirm>();
            var invoiceNumMapCoursePrice = new Dictionary<string, decimal>();
            var awaitMakeUpLessons = new List<AwaitMakeUpLesson>();
            if (inputObj.IsInvoiceChange)
            {
                try
                {
                    invoices = await _ablemusicContext.Invoice.Where(i => i.IsActive == 1 && invoiceNumsMapLessonQuantity.Keys.Contains(i.InvoiceNum)).ToListAsync();
                    invoiceWaitingConfirms = await _ablemusicContext.InvoiceWaitingConfirm.Where(iw => iw.IsActivate == 1 && invoiceNumsMapLessonQuantity.Keys.Contains(iw.InvoiceNum)).ToListAsync();
                    foreach(var i in invoiceWaitingConfirms)
                    {
                        var coursePrice = (await _ablemusicContext.One2oneCourseInstance.Where(oto => oto.CourseInstanceId == i.CourseInstanceId).Include(oto => oto.Course).FirstOrDefaultAsync()).Course.Price;
                        invoiceNumMapCoursePrice.Add(i.InvoiceNum, coursePrice.Value);
                    }
                }
                catch(Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
                if(invoiceWaitingConfirms.Count <= 0 || invoiceWaitingConfirms.Count < invoiceNumsMapLessonQuantity.Count)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Invoce not found, try to request without apply to invoce";
                    return BadRequest(result);
                }
                foreach(var i in invoices)
                {
                    if(i.PaidFee > 0)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Paid invoce found, try to request without apply to invoce";
                        result.Data = new List<object>
                        {
                            i
                        };
                        return BadRequest(result);
                    } else
                    {
                        var fee = invoiceNumMapCoursePrice[i.InvoiceNum] * invoiceNumsMapLessonQuantity[i.InvoiceNum];
                        i.LessonFee -= fee;
                        i.TotalFee -= fee;
                        i.OwingFee -= fee;
                        i.LessonQuantity += invoiceNumsMapLessonQuantity[i.InvoiceNum];
                    }
                }
                foreach (var iw in invoiceWaitingConfirms)
                {
                    if (iw.PaidFee > 0)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Paid invoce found, try to request without apply to invoce";
                        result.Data = new List<object>
                        {
                            iw
                        };
                        return BadRequest(result);
                    }
                    else
                    {
                        var fee = invoiceNumMapCoursePrice[iw.InvoiceNum] * invoiceNumsMapLessonQuantity[iw.InvoiceNum];
                        iw.LessonFee -= fee;
                        iw.TotalFee -= fee;
                        iw.OwingFee -= fee;
                        iw.LessonQuantity -= invoiceNumsMapLessonQuantity[iw.InvoiceNum];
                    }
                }
            } else
            {
                foreach(var l in lessons)
                {
                    awaitMakeUpLessons.Add(new AwaitMakeUpLesson()
                    {
                        CreateAt = DateTime.UtcNow.ToNZTimezone(),
                        SchduledAt = null,
                        ExpiredDate = l.BeginTime.Value.AddMonths(3).Date,
                        MissedLessonId = l.LessonId,
                        NewLessonId = null,
                        IsActive = 1,
                        LearnerId = l.LearnerId,
                        CourseInstanceId = l.CourseInstanceId,
                        GroupCourseInstanceId = null,
                        Remaining = remaining
                    });
                }
            }

            foreach(var cs in courseSchedules)
            {
                if(cs.LearnerId != inputObj.LearnerId)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "InstanceId not match learnerId";
                    return BadRequest(result);
                }
                Amendment amendment = new Amendment
                {
                    CourseInstanceId = cs.CourseInstanceId,
                    OrgId = cs.OrgId,
                    DayOfWeek = cs.DayOfWeek,
                    BeginTime = null,
                    EndTime = null,
                    LearnerId = cs.LearnerId,
                    RoomId = cs.RoomId,
                    BeginDate = inputObj.BeginDate,
                    EndDate = inputObj.EndDate,
                    CreatedAt = toNZTimezone(DateTime.UtcNow),
                    Reason = inputObj.Reason,
                    IsTemporary = null,
                    AmendType = 1,
                    CourseScheduleId = cs.CourseScheduleId
                };
                amendments.Add(amendment);
            }

            DateTime todoDate = inputObj.EndDate;
            foreach (var holiday in holidays)
            {
                if (holiday.HolidayDate.Date == todoDate.Date)
                {
                    todoDate = todoDate.AddDays(-1);
                }
            }

            DateTime remindScheduledDate = todoDate;

            var teacherIdMapTodoContent = new Dictionary<short, string>();
            var teacherMapRemindLogContent = new Dictionary<Teacher, string>();

            foreach (var cs in courseSchedules)
            {
                if (!teacherIdMapTodoContent.ContainsKey(cs.TeacherId))
                {
                    teacherIdMapTodoContent.Add(cs.TeacherId, TodoListContentGenerator.DayOffForTeacher(cs, inputObj.EndDate.ToString()));
                }

                teacherMapRemindLogContent.Add(new Teacher
                {
                    TeacherId = cs.TeacherId,
                    Email = cs.TeacherEmail
                }, RemindLogContentGenerator.DayOffForTeacher(cs, inputObj.EndDate.ToString()));
            }

            using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
            {
                TodoRepository todoRepository = new TodoRepository(_ablemusicContext);
                todoRepository.AddSingleTodoList("Period Dayoff Remind", TodoListContentGenerator.DayOffForLearner(courseSchedules[0],
                    inputObj.EndDate.ToString()), inputObj.UserId, todoDate, null, courseSchedules[0].LearnerId, null);
                todoRepository.AddMutipleTodoLists("Period Dayoff Remind", teacherIdMapTodoContent, inputObj.UserId, todoDate, null, null);
                var saveTodoResult = await todoRepository.SaveTodoListsAsync();
                if (!saveTodoResult.IsSuccess)
                {
                    return BadRequest(saveTodoResult);
                }

                RemindLogRepository remindLogRepository = new RemindLogRepository(_ablemusicContext);
                remindLogRepository.AddSingleRemindLog(courseSchedules[0].LearnerId, courseSchedules[0].LearnerEmail,
                    RemindLogContentGenerator.DayOffForLearner(courseSchedules[0], inputObj.EndDate.ToString()), null, "Period Dayoff Remind", null, remindScheduledDate);
                remindLogRepository.AddMultipleRemindLogs(teacherMapRemindLogContent, null, "Period Dayoff Remind", null, remindScheduledDate);
                var saveRemindLogResult = await remindLogRepository.SaveRemindLogAsync();
                if (!saveRemindLogResult.IsSuccess)
                {
                    return BadRequest(saveRemindLogResult);
                }

                try
                {
                    foreach (var amendment in amendments)
                    {
                        await _ablemusicContext.Amendment.AddAsync(amendment);
                    }
                    if (!inputObj.IsInvoiceChange)
                    {
                        foreach(var makeUpLesson in awaitMakeUpLessons)
                        {
                            await _ablemusicContext.AwaitMakeUpLesson.AddAsync(makeUpLesson);
                        }
                    }
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
                //List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
                foreach (var todo in saveTodoResult.Data)
                {
                    var remind = saveRemindLogResult.Data.Find(r => r.LearnerId == todo.LearnerId && r.TeacherId == todo.TeacherId);
                    string currentPersonName = "";
                    dynamic currentCourseSchedule = null;
                    if (todo.TeacherId == null)
                    {
                        foreach (var cs in courseSchedules)
                        {
                            if (todo.LearnerId == cs.LearnerId)
                            {
                                currentPersonName = cs.LearnerFirstName + " " + cs.LearnerLastName;
                                currentCourseSchedule = cs;
                            }
                        }
                    }
                    else
                    {
                        foreach (var cs in courseSchedules)
                        {
                            if (todo.TeacherId == cs.TeacherId)
                            {
                                currentPersonName = cs.TeacherFirstName + " " + cs.TeacherLastName;
                                currentCourseSchedule = cs;
                            }
                        }
                    }
                    string confirmURL = userConfirmUrlPrefix + todo.ListId + "/" + remind.RemindId;
                    string mailContent = EmailContentGenerator.Dayoff(currentPersonName, currentCourseSchedule, inputObj, confirmURL);
                    remindLogRepository.UpdateContent(remind.RemindId, mailContent);
                    //notifications.Add(new NotificationEventArgs(remind.Email, "Dayoff is expired", mailContent, remind.RemindId));
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

            foreach (var amendment in amendments)
            {
                amendment.Learner = null;
            }
            result.Data = new 
            {   
                EffectedAmendmentCount = amendments.Count,
                EffectedLessonsCount = lessons.Count,
                EffectedAwaitMakeUpLessonCount = awaitMakeUpLessons.Count,
                EffectedInvoiceWaitingConfirmCount = invoiceWaitingConfirms.Count,
                EffectedInvoiceCount = invoices.Count,
                EffectedAmendments = amendments,
                EffectedLessons = lessons,
                EffectedAwaitMakeUpLessons = awaitMakeUpLessons,
                EffectedInvoiceWaitingConfirm = invoiceWaitingConfirms,
                EffectedInvoices = invoices,
            };
            return Ok(result);
        }
    }
}
