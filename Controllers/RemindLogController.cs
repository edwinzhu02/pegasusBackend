using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemindLogController: BasicController
    {
        public RemindLogController(ablemusicContext ablemusicContext, ILogger<RemindLogController> log) : base(ablemusicContext, log)
        {
        }

        [HttpGet("{beginDate}/{endDate}")]
        public async Task<IActionResult> GetRemindLog(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<Object>();
            try
            {
                var item = await _ablemusicContext.RemindLog
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Lesson)
                    .ThenInclude(l=>l.CourseInstance)                    
                    .ThenInclude(ci=>ci.Course)                                        
                    .Include(s=>s.Lesson)
                    .ThenInclude(l=>l.GroupCourseInstance)                    
                    .ThenInclude(gc=>gc.Course)
                    .Include(s=>s.Lesson)
                    .ThenInclude(l=>l.TrialCourse)                    
                    .Where(s => beginDate.Value.Date <= s.CreatedAt.Value.Date &&
                                s.CreatedAt.Value.Date <= endDate.Value.Date)
                    .Select(s=> new
                    {
                        s.RemindId,s.Email,s.RemindType,s.RemindContent,s.CreatedAt,s.IsLearner,s.ProcessFlag,
                        s.EmailAt,s.RemindTitle,s.ReceivedFlag,
                        CourseName = IsNull(s.Lesson.GroupCourseInstance.Course.CourseName)?
                                    (IsNull(s.Lesson.CourseInstance.Course.CourseName)?
                                    s.Lesson.TrialCourse.CourseName:s.Lesson.CourseInstance.Course.CourseName):
                                    (s.Lesson.GroupCourseInstance.Course.CourseName),                        
                        Learner = IsNull(s.Learner)?
                            null:new {s.Learner.FirstName,s.Learner.LastName,s.LearnerId},
                        Teacher= IsNull(s.Teacher)?null:new {s.Teacher.TeacherId,s.Teacher.FirstName,s.Teacher.LastName
                        }
                    })
                    .ToListAsync();
                    result.Data = item;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
    }
}