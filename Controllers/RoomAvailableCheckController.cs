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
    public class RoomAvailableCheckController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public RoomAvailableCheckController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        // GET: api/RoomAvailableCheck/5
        [Route("checkbylesson/{orgId}/{startTimeStr}/{endTimeStr}")]
        [HttpGet]
        public async Task<IActionResult> Get(int orgId, string startTimeStr, string endTimeStr)
        {
            var result = new Result<Object>();
            var availableRooms = new List<Room>();
            var conflictRooms = new List<Lesson>();
            DateTime startTime;
            DateTime endTime;
            try
            {
                startTime = DateTime.Parse(startTimeStr);
                endTime = DateTime.Parse(endTimeStr);
                availableRooms = await _ablemusicContext.Room.Where(r => r.OrgId == orgId).ToListAsync();
                conflictRooms = await _ablemusicContext.Lesson.Where(l => l.OrgId == orgId && l.IsCanceled != 1 && 
                    ((l.BeginTime > startTime && l.BeginTime < endTime) ||
                    (l.EndTime > startTime && l.EndTime < endTime) ||
                    (l.BeginTime <= startTime && l.EndTime >= endTime) ||
                    (l.BeginTime > startTime && l.EndTime < endTime)))
                    .ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if(availableRooms.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room not found";
                return BadRequest(result);
            }
            if(startTime > endTime)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Start time should be smaller than end time";
                return BadRequest(result);
            }

            foreach(var a in availableRooms.Reverse<Room>())
            {
                if (conflictRooms.FindAll(c => c.RoomId == a.RoomId).Count > 0)
                {
                    availableRooms.Remove(a);
                }
            }

            result.Data = from a in availableRooms
                          select new
                          {
                              a.RoomId,
                              a.OrgId,
                              a.RoomName,
                              a.IsActivate,
                              a.CreatedAt,
                          };
            return Ok(result);
        }
    }
}
