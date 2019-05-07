using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController: BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public RoomController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
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
                var staff = _pegasusContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _pegasusContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var rooms = _pegasusContext.Room.Where(s => s.OrgId == orgId);
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
                result.Data = await _pegasusContext.Room
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