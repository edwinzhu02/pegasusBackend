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
    public class RemindLogController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public RemindLogController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
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
                    .Where(s => beginDate.Value.Date <= s.CreatedAt.Value.Date &&
                                s.CreatedAt.Value.Date <= endDate.Value.Date)
                    .Select(s=> new
                    {
                        s.RemindId,s.Email,s.RemindType,s.RemindContent,s.CreatedAt,s.IsLearner,s.ProcessFlag,
                        s.EmailAt,s.RemindTitle,s.ReceivedFlag,
                        Learner = IsNull(s.Learner)?
                            null:new {s.Learner.FirstName,s.Learner.LastName,s.LearnerId},
                        Teacher= IsNull(s.Teacher)?null:new {s.Teacher.TeacherId,s.Teacher.FirstName,s.Teacher.LastName}
                    })
                    .ToListAsync();
                return Ok(item);
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