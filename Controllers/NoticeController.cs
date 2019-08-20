using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController: BasicController
    {
        public NoticeController(ablemusicContext ablemusicContext, ILogger<NoticeController> log) : base(ablemusicContext, log)
        {       
        }

        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetNotices(int staffId)
        {
            var result = new Result<object>();
            try
            {
                var notices = await _ablemusicContext.Notices
                         .Where(s => s.ToStaffId==staffId && s.IsCompleted == 0)
                    .ToListAsync();
                result.Data = notices;
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