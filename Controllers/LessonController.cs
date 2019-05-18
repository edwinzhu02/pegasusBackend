using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;

        public LessonController(pegasusContext.ablemusicContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        
        //GET: http://localhost:5000/api/lesson/:date
        [HttpGet("{date}")]
        [Authorize]
        //for receptionist
        public async Task<IActionResult> GetLesson(DateTime date)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var userId = int.Parse(User.Claims.First(s => s.Type == "UserID").Value);
                var staff = _pegasusContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _pegasusContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var details = _pegasusContext.Lesson.Where(w => w.OrgId == orgId)
                    .Where(s=>s.BeginTime.Value.Year == date.Year && s.BeginTime.Value.Month == date.Month && s.BeginTime.Value.Day == date.Day)
                    .Include(s=>s.Learner)
                    .Include(s => s.Teacher)
                    .Include(group => group.GroupCourseInstance)
                    .ThenInclude(learnerCourse => learnerCourse.LearnerGroupCourse)
                    .ThenInclude(learner => learner.Learner)
                    .Select(s =>new {id = s.LessonId, resourceId = s.RoomId, start = s.BeginTime,end=s.EndTime,
                        title=IsNull(s.GroupCourseInstance)?"1 to 1":"Group Course",description="",
                        teacher=s.Teacher.FirstName.ToString(),
                        student=IsNull(s.GroupCourseInstance)?new List<string>(){s.Learner.FirstName}:s.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        IsGroup=!IsNull(s.GroupCourseInstance)
                    });
                    
                result.Data = await details.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);

        }

        [HttpGet("user/{id}")]
        public  IActionResult GetLessonsforteacher(byte id)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var teacher = _pegasusContext.Teacher.FirstOrDefault(s => s.UserId == id);
                if (teacher == null)
                {
                    throw new Exception("Teacher does not exist.");
                }
                var teacherId = teacher.TeacherId;
                var details = _pegasusContext.Lesson.Where(s => s.TeacherId == teacherId)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        title=IsNull(q.GroupCourseInstanceId)?"One to One":"Group",start=q.BeginTime,end=q.EndTime,
                        student=IsNull(q.GroupCourseInstance)?new List<string>(){q.Learner.FirstName}:q.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        description="", courseName=IsNull(q.GroupCourseInstanceId)?q.CourseInstance.Course.CourseName:q.GroupCourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName
                    });
                result.Data = details;

                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLesson()
        {
            var result = new Result<object>();

            var invoice = await _pegasusContext.Invoice.FirstOrDefaultAsync(x => x.InvoiceId == 1);
            var course = await _pegasusContext.One2oneCourseInstance.FirstOrDefaultAsync(x => x.CourseInstanceId == invoice.CourseInstanceId);

            var holiday = await _pegasusContext.Holiday.Select(x => x.HolidayDate).ToArrayAsync();
            var schedules = await _pegasusContext.CourseSchedule.Where(x => x.CourseInstanceId == invoice.CourseInstanceId).ToArrayAsync();
            var amendments = await _pegasusContext.Amendment.Where(x => x.CourseInstanceId == invoice.CourseInstanceId && x.BeginDate <= invoice.EndDate).ToArrayAsync();

            DateTime begindate_invoice = (DateTime)invoice.BeginDate;
            //get the day of week of the begindate in invoice
            int DayOfWeek_invoice = day_trans(begindate_invoice.DayOfWeek.ToString());

            DateTime[] lesson_begindate = new DateTime[schedules.Length];
            int[] num = new int[schedules.Length];

            int[] amen_week = new int[schedules.Length];

            for (int i = 0; i < invoice.LessonQuantity;)
            {
                int lesson_flag = 0;

                foreach (var schedule in schedules)
                {
                    int flag = 0;
                    int count = 0; //count the day between invoice begindate and course date

                    //calculated the begindate of the course
                    if (DayOfWeek_invoice >= (int)schedule.DayOfWeek)
                    {
                        count = (int)(7 - DayOfWeek_invoice + schedule.DayOfWeek);
                        lesson_begindate[lesson_flag] = begindate_invoice.AddDays(count + 7 * num[lesson_flag]);
                    }
                    else if (DayOfWeek_invoice < (int)schedule.DayOfWeek)
                    {
                        count = (int)(schedule.DayOfWeek - DayOfWeek_invoice);
                        lesson_begindate[lesson_flag] = begindate_invoice.AddDays(count + 7 * num[lesson_flag]);
                    }

                    lesson_begindate[lesson_flag] = Convert.ToDateTime(lesson_begindate[lesson_flag].ToString("yyyy-MM-dd"));

                    //begin to generate the lesson
                    try
                    {
                        Lesson lesson = new Lesson();
                        lesson.CourseInstanceId = invoice.CourseInstanceId;
                        lesson.CreatedAt = DateTime.Now;
                        lesson.RoomId = course.RoomId;
                        lesson.OrgId = (short)course.OrgId;
                        lesson.TeacherId = course.TeacherId;
                        lesson.LearnerId = (int)invoice.LearnerId;
                        lesson.InvoiceId = invoice.InvoiceId;

                        string begintime = "";
                        string endtime = "";
                        if (amendments != null)
                        {
                            foreach (var amendment in amendments)
                            {
                                if (lesson_begindate[lesson_flag] >= amendment.BeginDate && lesson_begindate[lesson_flag] <= amendment.EndDate)
                                {
                                    if (amendment.AmendType == 1)
                                    {
                                        while (lesson_begindate[lesson_flag] <= amendment.EndDate)
                                        {
                                            num[lesson_flag]++;
                                            lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(7);
                                        }

                                    }

                                    else if (amendment.AmendType == 2 && schedule.CourseScheduleId == amendment.CourseScheduleId)
                                    {
                                        count = 0;
                                        if (schedule.DayOfWeek > (int)amendment.DayOfWeek)
                                        {
                                            count = (int)(7 - schedule.DayOfWeek + amendment.DayOfWeek);
                                            lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(count);
                                        }
                                        else if (schedule.DayOfWeek <= (int)amendment.DayOfWeek)
                                        {
                                            count = (int)(amendment.DayOfWeek - schedule.DayOfWeek);
                                            lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(count);
                                        }
                                        begintime = amendment.BeginTime.ToString();
                                        endtime = amendment.EndTime.ToString();
                                        flag = 1;
                                    }
                                }
                            }
                        }

                        //if the lesson date is holiday, then skip this date
                        if (holiday != null)
                        {
                            foreach (var ele in holiday)
                            {
                                Boolean is_Equal = string.Equals(lesson_begindate[lesson_flag].ToString("yyyy-MM-dd"), ele.ToString("yyyy-MM-dd"));
                                if (is_Equal == true)
                                {
                                    num[lesson_flag]++;
                                    lesson_begindate[lesson_flag] = lesson_begindate[lesson_flag].AddDays(7);
                                }
                            }
                        }

                        if (lesson_begindate[lesson_flag] > invoice.EndDate) { i = (int)invoice.LessonQuantity; break; }
                        string lesson_begindate_result = lesson_begindate[lesson_flag].ToString("yyyy-MM-dd");
                        //Concat the datetime, date from invoice and time from schedule
                        if (flag == 0)
                        {
                            begintime = schedule.BeginTime.ToString();
                            endtime = schedule.EndTime.ToString();

                        }

                        string beginDate = string.Concat(lesson_begindate_result, " ", begintime);

                        string endDate = string.Concat(lesson_begindate_result, " ", endtime);
                        DateTime BeginTime = Convert.ToDateTime(beginDate);
                        DateTime EndTime = Convert.ToDateTime(endDate);
                        lesson.BeginTime = BeginTime;
                        lesson.EndTime = EndTime;
                        await _pegasusContext.Lesson.AddAsync(lesson);
                        //await _pegasusContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        result.ErrorMessage = e.Message;
                        result.IsSuccess = false;
                        result.IsFound = false;
                        return BadRequest(result);
                    }
                    i++;
                    num[lesson_flag]++;
                    lesson_flag++;
                }
            }
            await _pegasusContext.SaveChangesAsync();
            return Ok(result);
        }

        private int day_trans(string day)
        {
            int day_num = 0;
            switch (day)
            {
                case "Monday":
                    day_num = 1;
                    break;
                case "Tuesday":
                    day_num = 2;
                    break;
                case "Wednesday":
                    day_num = 3;
                    break;
                case "Thursday":
                    day_num = 4;
                    break;
                case "Friday":
                    day_num = 5;
                    break;
                case "Saturday":
                    day_num = 6;
                    break;
                case "Sunday":
                    day_num = 7;
                    break;
            }

            return day_num;
        }

    }
}