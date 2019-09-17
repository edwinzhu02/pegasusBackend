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
        [HttpPut("{staffId}/{noticeId}/{type}")]
        public async Task<IActionResult> ReadNotices(int staffId,int noticeId,short type){

            var result = new Result<object>();
            try
            {
                var notice = _ablemusicContext.Notices
                    .FirstOrDefault(n => n.NoticeId == noticeId);
                if (notice == null) throw new Exception("This notice is not exists");
                
                if (type ==1 ){
                    notice.IsCompleted=1;
                    notice.IsRead =1 ;
                }
                else{
                    notice.IsRead =1;
                }

                _ablemusicContext.Update(notice);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Notices(Notices notice){

            var result = new Result<object>();
            notice.IsRead =0;
            notice.IsCompleted =0;
            notice.CreatedAt = toNZTimezone(DateTime.UtcNow);
            try
            {
                if (notice.FromStaffId ==null|| notice.ToStaffId==null||notice.Notice==null ){
                    throw new Exception("Notice data error!");
                }
                await _ablemusicContext.AddAsync(notice);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }

 
        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetNotices(int staffId)
        {
            var result = new Result<object>();
            try
            {
                var notices = await _ablemusicContext.Notices.Include(n => n.FromStaff)
                         .Where(s => s.ToStaffId==staffId && s.IsCompleted == 0).
                         Select(n => new{
                             n.NoticeId,n.CreatedAt,n.Notice,n.ToStaffId,n.IsCompleted,n.IsRead,
                            n.FromStaffId,n.FromStaff.FirstName,n.FromStaff.LastName
                         })
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