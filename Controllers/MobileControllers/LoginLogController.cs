using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Utilities;
namespace Pegasus_backend.Controllers.MobileControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginLogController: BasicController
    {
        public LoginLogController(ablemusicContext ablemusicContext, ILogger<LessonRescheduleController> log) : base(ablemusicContext, log){}

        [HttpGet]
        public async Task<IActionResult> LoginLog()
        {
            var result = new Result<object>();
            try
            {
                result.Data = await _ablemusicContext.LoginLog
                    .Include(s=>s.Org)
                    .Select(s=>new{s.LogType,s.CreatedAt,s.Org.Abbr})
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInOrOutModel model)
        {
            var result = new Result<string>();
            try
            {
                var Org = _ablemusicContext.Org.FirstOrDefault(s =>
                    Math.Sqrt(Convert.ToDouble((s.LocaltionX - model.LocaltionX) * (s.LocaltionX - model.LocaltionX) +
                                               (s.LocaltionY - model.LocaltionY) *
                                               (s.LocaltionY - model.LocaltionY))) <= 0.003
                );
                if (Org == null)
                {
                    throw new Exception("Check in failed. 你必须在校区列表里随便一个校区的300米内");
                }
                var newLogLog = new LoginLog
                {
                    UserId = model.UserId,
                    LogType = 1,
                    CreatedAt = DateTime.UtcNow.AddHours(12),
                    OrgId = Org.OrgId
                };
                _ablemusicContext.Add(newLogLog);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = "Check in successfully.";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CheckOut(CheckInOrOutModel model)
        {
            var result = new Result<string>();
            try
            {
                var checkInDetail = _ablemusicContext.LoginLog.FirstOrDefault(s =>
                    s.CreatedAt.Value.Day == DateTime.Now.Day &&
                    s.CreatedAt.Value.Month == DateTime.Now.Month &&
                    s.CreatedAt.Value.Year == DateTime.Now.Year && s.LogType == 1);
                if (checkInDetail == null)
                {
                    
                }
                return Ok();
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