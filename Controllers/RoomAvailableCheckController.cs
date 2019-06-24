using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomAvailableCheckController : BasicController
    {
        public RoomAvailableCheckController(ablemusicContext ablemusicContext, ILogger<RoomAvailableCheckController> log) : base(ablemusicContext, log)
        {
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
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if (availableRooms.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room not found";
                return BadRequest(result);
            }
            if (startTime > endTime)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Start time should be smaller than end time";
                return BadRequest(result);
            }

            foreach (var a in availableRooms.Reverse<Room>())
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

        [Route("checkbyinstance/{orgId}/{startTimeStr}/{endTimeStr}/{dayOfWeek}/{startDateStr?}")]
        [HttpGet]
        public async Task<IActionResult> CheckByInstanceGet(int orgId, string startTimeStr, string endTimeStr, int dayOfWeek, string startDateStr = null)
        {
            var result = new Result<Object>();
            TimeSpan startTime;
            TimeSpan endTime;
            List<Room> availableRooms = new List<Room>();
            List<Room> conflictInGroupCourse;
            List<Amendment> conflictInOtOCourse;
            List<Amendment> changedCourseAmendments;
            List<Amendment> changedCoursePermanentExpiredAmendments;
            var conflictRooms = new List<Room>();
            try
            {
                DateTime startDate = startDateStr == null ? toNZTimezone(DateTime.UtcNow) : DateTime.Parse(startDateStr);
                startTime = TimeSpan.Parse(startTimeStr);
                endTime = TimeSpan.Parse(endTimeStr);
                availableRooms = await _ablemusicContext.Room.Where(r => r.OrgId == orgId).ToListAsync();
                conflictInGroupCourse = await (from cs in _ablemusicContext.CourseSchedule
                                               join g in _ablemusicContext.GroupCourseInstance on cs.GroupCourseInstanceId equals g.GroupCourseInstanceId
                                               where g.OrgId == orgId && cs.DayOfWeek == dayOfWeek && (startDate <= g.EndDate || g.EndDate == null) &&
                                               ((cs.BeginTime < startTime && startTime < cs.EndTime) || (cs.BeginTime < endTime && endTime < cs.EndTime) ||
                                               (startTime <= cs.BeginTime && endTime >= cs.EndTime))
                                               select new Room
                                               {
                                                   RoomId = g.RoomId.Value
                                               }
                                               ).ToListAsync();
                conflictInOtOCourse = await (from cs in _ablemusicContext.CourseSchedule
                                             join oto in _ablemusicContext.One2oneCourseInstance on cs.CourseInstanceId equals oto.CourseInstanceId
                                             where oto.OrgId == orgId && cs.DayOfWeek == dayOfWeek && (startDate <= oto.EndDate || oto.EndDate == null) &&
                                               ((cs.BeginTime < startTime && startTime < cs.EndTime) || (cs.BeginTime < endTime && endTime < cs.EndTime) ||
                                               (startTime <= cs.BeginTime && endTime >= cs.EndTime))
                                             select new Amendment
                                             {
                                                 RoomId = oto.RoomId,
                                                 CourseScheduleId = cs.CourseScheduleId
                                             }
                                             ).ToListAsync();
                changedCourseAmendments = await _ablemusicContext.Amendment.Where(a => a.OrgId == orgId && a.AmendType == 2 && a.DayOfWeek == dayOfWeek &&
                                                a.EndDate >= startDate && (a.IsTemporary == 1 || (a.IsTemporary == 0 && startDate <= a.BeginDate)) &&
                                                ((a.BeginTime < startTime && startTime < a.EndTime) || 
                                                (a.BeginTime < endTime && endTime < a.EndTime) ||
                                                (startTime <= a.BeginTime && endTime >= a.EndTime))).ToListAsync();
                changedCoursePermanentExpiredAmendments = await _ablemusicContext.Amendment.Where(a => a.OrgId == orgId && a.AmendType == 2 &&
                a.IsTemporary == 0 && startDate > a.BeginDate).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            foreach(var ca in changedCourseAmendments)
            {
                conflictRooms.Add(new Room
                {
                    RoomId = ca.RoomId.Value
                });
            }

            foreach(var ccpea in changedCoursePermanentExpiredAmendments)
            {
                if ((ccpea.BeginTime < startTime && startTime < ccpea.EndTime) || (ccpea.BeginTime < endTime && endTime < ccpea.EndTime) ||
                    (startTime <= ccpea.BeginTime && endTime >= ccpea.EndTime))
                {
                    conflictRooms.Add(new Room
                    {
                        RoomId = ccpea.RoomId.Value
                    });
                }
                foreach(var cotoc in conflictInOtOCourse.Reverse<Amendment>())
                {
                    if (cotoc.CourseScheduleId == ccpea.CourseScheduleId)
                    {
                        conflictInOtOCourse.Remove(cotoc);
                    }
                }
            }

            foreach (var cgc in conflictInGroupCourse)
            {
                conflictRooms.Add(new Room
                {
                    RoomId = cgc.RoomId
                });
            }

            foreach (var cotoc in conflictInOtOCourse)
            {
                conflictRooms.Add(new Room
                {
                    RoomId = cotoc.RoomId.Value
                });
            }

            foreach (var a in availableRooms.Reverse<Room>())
            {
                if (conflictRooms.FindAll(cr => a.RoomId == cr.RoomId).Count > 0)
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
