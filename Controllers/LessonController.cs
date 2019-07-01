using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController: BasicController
    {

        public LessonController(ablemusicContext ablemusicContext, ILogger<LessonController> log) : base(ablemusicContext, log)
        {
        }

        /*[HttpGet]
        [Route("abc")]
        public IActionResult a()
        {
            var item = _ablemusicContext.Lesson
                .Include(s => s.CourseInstance)
                .Include(s => s.Learner)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Include(s => s.Org)
                .Include(s => s.TrialCourse)
                .Include(group => group.GroupCourseInstance)
                .ThenInclude(learnerCourse => learnerCourse.LearnerGroupCourse)
                .ThenInclude(learner => learner.Learner)
                .FirstOrDefault(s => s.LessonId == 2);


            var result = IsNull(item.CourseInstanceId)
                ? false
                : _ablemusicContext.Fund.FirstOrDefault(q => q.LearnerId == item.LearnerId).Balance
                  - _ablemusicContext.Course.FirstOrDefault(q => q.CourseId == item.CourseInstance.CourseId).Price <= 0;
            return Ok(result);
        }*/
        
        //GET: http://localhost:5000/api/lesson/GetLessonsForReceptionist/:userId/:date
        [HttpGet("[action]/{userId}/{date}")]
        //for receptionist
        public async Task<IActionResult> GetLessonsForReceptionist(int userId,DateTime date)
        {
            Result<Object> result = new Result<Object>();
            try
            {
    
               
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _ablemusicContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var details = _ablemusicContext.Lesson
                    .Where(s=>s.IsCanceled != 1 && s.IsConfirm != 1)
                    .Where(s=>s.OrgId==orgId&&s.BeginTime.Value.Year == date.Year && s.BeginTime.Value.Month == date.Month && s.BeginTime.Value.Day == date.Day)
                    .Include(s=>s.CourseInstance)
                    .Include(s=>s.Learner)
                    .Include(s => s.Teacher)
                    .Include(s=>s.Room)
                    .Include(s=>s.Org)
                    .Include(s=>s.TrialCourse)
                    .Include(group => group.GroupCourseInstance)
                    .ThenInclude(learnerCourse => learnerCourse.LearnerGroupCourse)
                    .ThenInclude(learner => learner.Learner)
                    .Select(s =>new {id = s.LessonId, resourceId = s.RoomId, start = s.BeginTime,end=s.EndTime,
                        title=IsNull(s.GroupCourseInstance)?IsNull(s.CourseInstance)?"T":"1":"G",description="",
                        teacher=s.Teacher.FirstName.ToString(),
                        isOwnAfterLesson=IsNull(s.CourseInstanceId)?
                            0
                            :
                            (_ablemusicContext.Fund.FirstOrDefault(q=>q.LearnerId==s.LearnerId).Balance 
                            - _ablemusicContext.Course.FirstOrDefault(q=>q.CourseId==s.CourseInstance.CourseId).Price <= 0)?1:0
                       ,
                        student=IsNull(s.GroupCourseInstance)?IsNull(s.CourseInstance)?new List<string>{s.Learner.FirstName}:new List<string>{s.Learner.FirstName}:s.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        IsGroup=!IsNull(s.GroupCourseInstance),
                        info = new
                        {
                            s.Room.RoomName,s.RoomId,s.Org.OrgName,s.OrgId,s.TeacherId,TeacherName=s.Teacher.FirstName,s.BeginTime,s.EndTime,
                            CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName,
                            s.LessonId,courseId=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseId:IsNull(s.CourseInstance)?s.TrialCourseId:s.CourseInstance.Course.CourseId,s.LearnerId
                        }
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

        
        [HttpGet("[action]/{userId}/{beginDate}")]
        public async Task<IActionResult> GetLessonsForTeacher(byte userId, DateTime beginDate)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var endDate = beginDate.AddDays(6);
                var teacher = _ablemusicContext.Teacher.FirstOrDefault(s => s.UserId == userId);
                if (teacher == null)
                {
                    throw new Exception("Teacher does not exist.");
                }
                var teacherId = teacher.TeacherId;
                var details = _ablemusicContext.Lesson.Where(s => s.TeacherId == teacherId)
                    .Where(s=>s.IsCanceled != 1 && s.IsConfirm != 1)
                    .Where(s=>beginDate.Date <= s.EndTime.Value.Date && s.EndTime.Value.Date <= endDate.Date)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        title=IsNull(q.GroupCourseInstance)?IsNull(q.CourseInstance)?"T":"1":"G",start=q.BeginTime,end=q.EndTime,
                        student=IsNull(q.GroupCourseInstance)?IsNull(q.CourseInstance)?new List<string>{q.Learner.FirstName}:new List<string>{q.Learner.FirstName}:q.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        description="", CourseName=!IsNull(q.GroupCourseInstance)?q.GroupCourseInstance.Course.CourseName:IsNull(q.CourseInstance)?q.TrialCourse.CourseName:q.CourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName
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
        

        [HttpGet("[action]/{teacherId}")]
        public async Task<IActionResult> GetLessonsTeacherId(short teacherId)
        {
            Result<Object> result = new Result<object>();
            try
            {
               
                var details = _ablemusicContext.Lesson.Where(s => s.TeacherId == teacherId)
                    .Where(s=>s.IsCanceled ==0)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        title=IsNull(q.GroupCourseInstanceId)?"1":"G",start=q.BeginTime,end=q.EndTime,
                        student=IsNull(q.GroupCourseInstance)?new List<string>{q.Learner.FirstName}:q.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        description="", courseName=IsNull(q.GroupCourseInstanceId)?q.CourseInstance.Course.CourseName:q.GroupCourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName
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
        [HttpGet("[action]/{learnerId}")]
        public async Task<IActionResult> GetArrangedLessonsByLearner(int learnerId)
        {
            Result<Object> result = new Result<object>();
            try
            {
               
                var details = _ablemusicContext.Lesson.Where(s => s.LearnerId == learnerId)
                    .Where(s=>s.IsCanceled ==0 &&s.IsConfirm !=1)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        start=q.BeginTime,end=q.EndTime,
                        learnerFirstName=q.Learner.FirstName,learnerLastName=q.Learner.LastName,
                        teacherFirstName=q.Teacher.FirstName,TeacherLastName=q.Teacher.LastName,
                        courseName=IsNull(q.GroupCourseInstanceId)?q.CourseInstance.Course.CourseName:q.GroupCourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName
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
                
        [HttpGet("[action]/{userId}/{beginDate}/{endDate}")]

        public async Task<IActionResult> GetLessonsBetweenDate(DateTime? beginDate, DateTime? endDate, int? userId)
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _ablemusicContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var items = _ablemusicContext.Lesson
                    .Include(s => s.Teacher)
                    .Include(s => s.Learner)
                    .Include(s => s.Room)
                    .Include(s => s.Org)
                    .Include(s => s.GroupCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s => s.CourseInstance)
                    .ThenInclude(w => w.Course)
                    .Where(s => beginDate.Value.Date <= s.EndTime.Value.Date &&
                                s.EndTime.Value.Date <= endDate.Value.Date && s.OrgId == orgId)
                    .Select(s => new
                    {
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName,
                        TeacherFirstName = s.Teacher.FirstName, s.BeginTime, s.EndTime, s.LessonId,
                        Room = s.Room.RoomName, Branch = s.Org.OrgName, s.IsCanceled, CancelReson = s.Reason,
                        s.IsConfirm,
                        s.IsTrial, Learner = s.Learner.FirstName, Learners = "", s.LearnerId, s.RoomId, s.TeacherId,
                        s.OrgId,
                        courseId=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseId:IsNull(s.CourseInstance)?s.TrialCourseId:s.CourseInstance.Course.CourseId
                    });
                
                result.Data = await items.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            
            return Ok(result);
        }
    }
}