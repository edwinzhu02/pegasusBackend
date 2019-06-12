using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgsController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public OrgsController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        // GET: api/Orgs
        [HttpGet]
        public async Task<IActionResult> GetOrg()
        {
            Result<List<Pegasus_backend.pegasusContext.Org>> result = new Result<List<Pegasus_backend.pegasusContext.Org>>();
            List<Pegasus_backend.pegasusContext.Org> orgs = new List<Pegasus_backend.pegasusContext.Org>();
            try
            {
                orgs = await _ablemusicContext.Org.ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }
            if(orgs == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "org not found";
            }
            result.Data = orgs;

            return Ok(result);
        }

        [Route("OrgAndRoom")]
        [HttpGet]
        public async Task<IActionResult> GetOrgAndRoom()
        {
            var result = new Result<List<Object>>();
            var orgs = new List<pegasusContext.Org>();
            var rooms = new List<Room>();
            try
            {
                orgs = await _ablemusicContext.Org.ToListAsync();
                rooms = await _ablemusicContext.Room.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }
            if (orgs == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "org not found";
            }
            if (rooms == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room not found";
                return BadRequest(result);
            }

            result.Data = new List<Object>();
            foreach(var o in orgs)
            {
                var eachOrgRooms = new List<Object>();
                foreach(var r in rooms)
                {
                    if (r.OrgId == o.OrgId)
                    {
                        eachOrgRooms.Add(new
                        {
                            RoomId = r.RoomId,
                            RoomName = r.RoomName,
                        });
                    }
                }
                result.Data.Add(new
                {
                    o.OrgId,
                    o.OrgName,
                    o.IsHeadoffice,
                    o.IsActivate,
                    o.Address,
                    o.Email,
                    o.Phone,
                    o.LocaltionX,
                    o.LocaltionY,
                    o.Abbr,
                    Rooms = eachOrgRooms
                });
            }

            return Ok(result);
        }

        private bool OrgExists(short id)
        {
            return _ablemusicContext.Org.Any(e => e.OrgId == id);
        }
    }
}