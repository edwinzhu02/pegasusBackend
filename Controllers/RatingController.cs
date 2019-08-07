using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Utilities;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController: BasicController
    {
        public RatingController(ablemusicContext ablemusicContext, ILogger<LessonRescheduleController> log) : base(ablemusicContext, log){}

        [HttpGet("[action]/{teacherId}")]
        public async Task<IActionResult> TeacherGetRating(short teacherId)
        {
            var result = new Result<object>();
            try
            {
                //0 is student to teacher 1 is teacher to student 2 is teacher to school
                var ratingItem = await _ablemusicContext.Rating
                    .Include(s=>s.Learner)
                    .Include(s=>s.Lesson)
                    .Where(s => s.TeacherId == teacherId && s.RateType == 0)
                    .Select(s=>new {s.Learner.FirstName,s.Learner.LastName,s.Lesson.BeginTime,s.Comment,s.RateStar})
                    .ToListAsync();
                result.Data = ratingItem;
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