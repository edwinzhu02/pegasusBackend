using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
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
        
        
        //GET: http://localhost:5000/api/lesson/GetLessonsForReceptionist/:userId/:date
        [HttpGet("[action]/{userId}/{date}")]
        //for receptionist
        public async Task<IActionResult> GetLessonsForReceptionist(int userId,DateTime date)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var staff = _pegasusContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _pegasusContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var details = _pegasusContext.Lesson
                    .Where(s=>s.IsCanceled != 1 && s.IsConfirm != 1)
                    .Where(s=>s.OrgId==orgId&&s.BeginTime.Value.Year == date.Year && s.BeginTime.Value.Month == date.Month && s.BeginTime.Value.Day == date.Day)
                    .Include(s=>s.Learner)
                    .Include(s => s.Teacher)
                    .Include(s=>s.Room)
                    .Include(s=>s.Org)
                    .Include(group => group.GroupCourseInstance)
                    .ThenInclude(learnerCourse => learnerCourse.LearnerGroupCourse)
                    .ThenInclude(learner => learner.Learner)
                    .Select(s =>new {id = s.LessonId, resourceId = s.RoomId, start = s.BeginTime,end=s.EndTime,
                        title=IsNull(s.GroupCourseInstance)?"1":"G",description="",
                        teacher=s.Teacher.FirstName.ToString(),
                        student=IsNull(s.GroupCourseInstance)?new List<string>{s.Learner.FirstName}:s.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        IsGroup=!IsNull(s.GroupCourseInstance),
                        info = new
                        {
                            s.Room.RoomName,s.RoomId,s.Org.OrgName,s.OrgId,s.TeacherId,TeacherName=s.Teacher.FirstName,s.BeginTime,s.EndTime,
                            CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:s.CourseInstance.Course.CourseName,
                            s.LessonId,courseId=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseId:s.CourseInstance.Course.CourseId,s.LearnerId
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

        
        //GET api/lesson/:lessonId
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var item = _pegasusContext.Lesson
                    .Include(s => s.Teacher)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Room)
                    .Include(s=>s.Org)
                    .Include(s=>s.GroupCourseInstance)
                    .ThenInclude(w=>w.Course)
                    .Include(s=>s.CourseInstance)
                    .ThenInclude(w=>w.Course)
                    .Select(s => new
                    {
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:s.CourseInstance.Course.CourseName,
                        TeacherFirstName=s.Teacher.FirstName,s.BeginTime,s.EndTime,s.LessonId,
                        Room=s.Room.RoomName, Branch=s.Org.OrgName, s.IsCanceled, CancelReson =s.Reason,
                        s.IsTrial,Learner = s.Learner.FirstName, Learners= ""
                    })
                    .FirstOrDefault(s => s.LessonId == id);
                result.Data = item;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
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
                var teacher = _pegasusContext.Teacher.FirstOrDefault(s => s.UserId == userId);
                if (teacher == null)
                {
                    throw new Exception("Teacher does not exist.");
                }
                var teacherId = teacher.TeacherId;
                var details = _pegasusContext.Lesson.Where(s => s.TeacherId == teacherId)
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
        

        [HttpGet("[action]/{teacherId}")]
        public async Task<IActionResult> GetLessonsTeacherId(short teacherId)
        {
            Result<Object> result = new Result<object>();
            try
            {
               
                var details = _pegasusContext.Lesson.Where(s => s.TeacherId == teacherId)
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
               
                var details = _pegasusContext.Lesson.Where(s => s.LearnerId == learnerId)
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
                var staff = _pegasusContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _pegasusContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var items = _pegasusContext.Lesson
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
                        CourseName = !IsNull(s.GroupCourseInstance)
                            ? s.GroupCourseInstance.Course.CourseName
                            : s.CourseInstance.Course.CourseName,
                        TeacherFirstName = s.Teacher.FirstName, s.BeginTime, s.EndTime, s.LessonId,
                        Room = s.Room.RoomName, Branch = s.Org.OrgName, s.IsCanceled, CancelReson = s.Reason,
                        s.IsConfirm,
                        s.IsTrial, Learner = s.Learner.FirstName, Learners = "", s.LearnerId, s.RoomId, s.TeacherId,
                        s.OrgId,
                        courseId = !IsNull(s.GroupCourseInstance)
                            ? s.GroupCourseInstance.Course.CourseId
                            : s.CourseInstance.Course.CourseId
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