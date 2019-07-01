using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseInstanceController : BasicController
    {

        public CourseInstanceController(ablemusicContext ablemusicContext, ILogger<CourseInstanceController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/CourseInstance/1/0
        [HttpGet("{id}/{type}")]
        public async Task<IActionResult> GetCourseInstance(int id, int type)
        {
            Result <One2oneCourseInstance> otoResult = new Result<One2oneCourseInstance>();
            Result<GroupCourseInstance> gResult = new Result<GroupCourseInstance>();
  
            if (type == 0)
            {
                try
                {
                    otoResult.Data = await _ablemusicContext.One2oneCourseInstance.Where(o => o.CourseInstanceId == id).Include(o => o.Course).FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    otoResult.IsSuccess = false;
                    otoResult.ErrorMessage = ex.Message;
                    return BadRequest(otoResult);
                }
                if (otoResult.Data == null)
                {
                    otoResult.IsFound = false;
                    otoResult.IsSuccess = false;
                    otoResult.ErrorMessage = "One2One instance id not found";
                    return NotFound(otoResult);
                }
                return Ok(otoResult);
            } else if (type == 1)
            {
                try
                {
                    gResult.Data = await _ablemusicContext.GroupCourseInstance.Where(g => g.GroupCourseInstanceId == id).Include(g => g.Course).FirstOrDefaultAsync();
                }
                catch(Exception ex)
                {
                    gResult.IsSuccess = false;
                    gResult.ErrorMessage = ex.Message;
                    return BadRequest(gResult);
                }
                if (gResult.Data == null)
                {
                    gResult.IsFound = false;
                    gResult.IsSuccess = false;
                    gResult.ErrorMessage = "Group course instance id not found";
                    return NotFound(gResult);
                }
                return Ok(gResult);
            } else
            {
                gResult.IsSuccess = false;
                gResult.ErrorMessage = "Unknown type provided";
                return BadRequest(gResult);
            }

        }

    }
}
