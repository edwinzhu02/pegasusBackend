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
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public LessonController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        
        //GET: http://localhost:5000/api/lesson
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLesson()
        {
            Result<Object> result = new Result<object>();
            try
            {
                var userId = int.Parse(User.Claims.First(s => s.Type == "UserID").Value);
                var staff = _pegasusContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _pegasusContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var details = _pegasusContext.Lesson.Where(w => w.OrgId == orgId)
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
    }
}