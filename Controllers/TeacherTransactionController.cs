using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherTransactionController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public TeacherTransactionController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetTeacherTransaction(short userId)
        {
            var result = new Result<Object>();
            try
            {
                result.Data = await _ablemusicContext.TeacherTransaction
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Lesson)
                    .ThenInclude(s=>s.Org)
                    .Where(s=>s.TeacherId==_ablemusicContext.Teacher.FirstOrDefault(w=>w.UserId==userId).TeacherId)
                    .Select(s=>new {TeacherFirstName=s.Teacher.FirstName,s.Teacher.TeacherId,
                        LessonBeginTime=s.Lesson.BeginTime,LessonEndTime=s.Lesson.EndTime,
                        s.LessonId, s.WageAmount, s.TranId, Branch=s.Lesson.Org.OrgName,
                        CourseName=IsNull(s.Lesson.GroupCourseInstanceId)?s.Lesson.CourseInstance.Course.CourseName:s.Lesson.GroupCourseInstance.Course.CourseName,
                        
                    })
                    .ToListAsync();
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