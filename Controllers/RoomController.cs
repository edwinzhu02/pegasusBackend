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
    public class RoomController: BasicController
    {
        public RoomController(ablemusicContext ablemusicContext, ILogger<RoomController> log) : base(ablemusicContext, log)
        {       
        }

        [HttpGet("{TeacherId}/{OrgId}/{beginTime}")]
        public async Task<IActionResult> getRoomsbyTeacherId(int TeacherId,short OrgId,DateTime beginTime)
        {
            var result = new Result<object>();
            try
            {
                var day = (int) beginTime.DayOfWeek==0?7:(int) beginTime.DayOfWeek;
                var availabledays = _ablemusicContext.AvailableDays
                    .Include(s => s.Room)
                    .Where(s => s.TeacherId == TeacherId && s.OrgId == OrgId && s.DayOfWeek == day)
                    .ToList();
                if (availabledays.Count == 0)
                {
                    result.Data = new List<object>();
                    return Ok(result);
                }

                if (availabledays.First().Room == null)
                {
                    result.Data = new List<object>();
                    return Ok(result);
                }

                var room = availabledays.Select(s => new
                {
                    s.Room.RoomId, s.Room.RoomName
                });
                result.Data = room;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        //GET: http://localhost:5000/api/room/forCalendar
        [Route("forCalendar")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetRoom()
        {
            Result<IEnumerable<Object>> result = new Result<IEnumerable<Object>>();
            try
            {
                var userId = int.Parse(User.Claims.First(s=>s.Type=="UserID").Value);
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _ablemusicContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var rooms = _ablemusicContext.Room.Where(s => s.OrgId == orgId);
                var data = rooms.Select(s =>  new {id=s.RoomId, title=s.RoomName });
                result.Data = await data.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }

        
        //GET: http://localhost:5000/api/room
        [HttpGet]
        public async Task<IActionResult> Room()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                result.Data = await _ablemusicContext.Room
                    .Include(s=>s.Org)
                    .Select(s=> new {RoomId=s.RoomId,RoomName=s.RoomName,OrgId=s.OrgId,OrgName=s.Org.OrgName} )
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