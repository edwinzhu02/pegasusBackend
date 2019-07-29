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
    public class TermController: BasicController
    {
        public TermController(ablemusicContext ablemusicContext, ILogger<RoomController> log) : base(ablemusicContext, log)
        {       
        }

        
        //GET: http://localhost:5000/api/room
        [HttpGet]
        public async Task<IActionResult> Term()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                result.Data = await _ablemusicContext.Term
                    .Select(t => new {t.TermId,t.TermName,t.BeginDate,t.EndDate})
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