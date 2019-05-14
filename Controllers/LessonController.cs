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
                        title=IsNull(s.GroupCourseInstance)?"One to One":"Group Course",description="",
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
        
    }
}