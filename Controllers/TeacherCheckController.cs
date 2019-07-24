using System;
using System.Collections.Generic;
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

    public class TeacherCheckController : BasicController
    {
        public TeacherCheckController(ablemusicContext ablemusicContext, ILogger<TeacherCheckController> log) : base(ablemusicContext, log)
        {

        }
        [HttpGet]
        public async Task<IActionResult> GetTeacher(short orgId, short cateId)
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var sOrg = await _ablemusicContext.AvailableDays
                .Where(s => s.OrgId == orgId)
                .Select(s => new {
                    s.TeacherId,
                    s.Teacher.FirstName,
                    s.Teacher.LastName
                }).ToListAsync();
                var sCate = await _ablemusicContext.TeacherCourse
                .Where( s => s.Course.CourseCategoryId == cateId)
                .Select(s => new{
                    s.TeacherId,
                    s.Teacher.FirstName,
                    s.Teacher.LastName
                    }).ToListAsync();
                var tid = sOrg.Intersect(sCate);

                if (tid == null)
                {
                    result.IsFound = false;
                }
                return Ok(tid);
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