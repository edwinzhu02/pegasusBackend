using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers.MobileControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileLoginLogsController : ControllerBase
    {
        private readonly ablemusicContext _context;
        private readonly IMapper _mapper;

        public MobileLoginLogsController(ablemusicContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/MobileLoginLogs
        [HttpGet]
        public IEnumerable<LoginLog> GetLoginLog()
        {
            return _context.LoginLog;
        }

        // GET: api/MobileLoginLogs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoginLog(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginLog = await _context.LoginLog.FindAsync(id);

            if (loginLog == null)
            {
                return NotFound();
            }

            return Ok(loginLog);
        }
        // get the lastest 4 loginLog
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetRecentLoginLog()
        {
            Result<List<LoginLog>> logResult = new Result<List<LoginLog>>();
            if (_context.LoginLog.Count() <= 4)
            {
                logResult.Data = await _context.LoginLog.Include(x => x.Org).ToListAsync();
                return Ok(logResult);
            }
            logResult.Data = await _context.LoginLog.OrderByDescending(x => x.LoginLogId).Take(4).Include(x =>x.Org).ToListAsync();
            return Ok(logResult);
        }
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetLastCheck(int id)
        {
            Result<LoginLog> logResult = new Result<LoginLog>();

            var checkIn = await _context.LoginLog.LastOrDefaultAsync(x => x.LogType == id);
            if (checkIn == null)
            {
                logResult.ErrorMessage = "Cannot find the last check details";
                logResult.IsSuccess = false;
                logResult.IsFound = false;
                return BadRequest(logResult);
            }

            logResult.Data = checkIn;
            return Ok(logResult);
        }

        // PUT: api/MobileLoginLogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoginLog([FromRoute] int id, [FromBody] LoginLog loginLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != loginLog.LoginLogId)
            {
                return BadRequest();
            }

            _context.Entry(loginLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoginLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MobileLoginLogs
        [CheckModelFilter]
        [HttpPost]
        public async Task<IActionResult> PostLoginLog(LoginLogModel loginLogModel)
        {
            Result<LoginLog> result = new Result<LoginLog>();
            LoginLog loginLog = new LoginLog();
            // change time zone
            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
            loginLogModel.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(loginLogModel.CreatedAt, timeInfo);
            _mapper.Map(loginLogModel, loginLog);
            try
            {
                _context.LoginLog.Add(loginLog);
                await _context.SaveChangesAsync();
                result.Data = loginLog;
                return Ok(result);
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return BadRequest(result);
            }
        }

        // DELETE: api/MobileLoginLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoginLog([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginLog = await _context.LoginLog.FindAsync(id);
            if (loginLog == null)
            {
                return NotFound();
            }

            _context.LoginLog.Remove(loginLog);
            await _context.SaveChangesAsync();

            return Ok(loginLog);
        }

        private bool LoginLogExists(int id)
        {
            return _context.LoginLog.Any(e => e.LoginLogId == id);
        }
    }
}