using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherFilterController : BasicController
    {
        public TeacherFilterController(ablemusicContext ablemusicContext, ILogger<TeacherFilterController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/TeacherFilter
        [HttpGet("{courseId}")]
        public async Task<IActionResult> Get(int courseId)
        {
            Result<List<Object>> result = new Result<List<Object>>();
            dynamic orgs;
            dynamic rooms;
            dynamic levels;
            dynamic teachers;

            try
            {
                orgs = await (from o in _ablemusicContext.Org
                              select new
                              {
                                  o.OrgId,
                                  o.OrgName,
                              }).ToListAsync();
                rooms = await (from r in _ablemusicContext.Room
                               select new
                               {
                                   r.RoomId,
                                   r.RoomName,
                                   r.OrgId
                               }).ToListAsync();
                levels = await (from l in _ablemusicContext.Lookup
                                where l.LookupType == 1
                                select new
                                {
                                    l.PropValue,
                                    l.PropName,
                                }).ToListAsync();
                teachers = await (from t in _ablemusicContext.Teacher
                                  join ta in _ablemusicContext.AvailableDays on t.TeacherId equals ta.TeacherId
                                  join tc in _ablemusicContext.TeacherCourse on t.TeacherId equals tc.TeacherId
                                  where tc.CourseId == courseId && t.IsActivate == 1
                                  select new
                                  {
                                      TeacherId = t.TeacherId,
                                      TeacherFirstName = t.FirstName,
                                      TeacherLastName = t.LastName,
                                      TeacherAvailableOrgId = ta.OrgId,
                                      TeacherLevelId = t.Level,
                                  } into x 
                                  group x by new
                                  {
                                      x.TeacherId,
                                      x.TeacherAvailableOrgId
                                  } into g
                                  select g.FirstOrDefault()
                                  ).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            List<Object> teacherResults = new List<Object>();
            foreach (var org in orgs)
            {
                List<Object> levelList = new List<Object>();
                foreach (var level in levels)
                {
                    List<Object> teacherList = new List<Object>();
                    foreach (var teacher in teachers)
                    {
                        if (teacher.TeacherAvailableOrgId == org.OrgId && teacher.TeacherLevelId == level.PropValue)
                        {
                            teacherList.Add(new
                            {
                                teacher.TeacherId,
                                TeacherName = teacher.TeacherFirstName + " " + teacher.TeacherLastName,
                            });
                        }
                    }

                    levelList.Add(new
                    {
                        levelId = level.PropValue,
                        levelName = level.PropName,
                        teacher = teacherList
                    });
                }
                List<Object> roomList = new List<object>();
                foreach (var room in rooms)
                {
                    if (org.OrgId == room.OrgId)
                    {
                        roomList.Add(new
                        {
                            room.RoomId,
                            room.RoomName
                        });
                    }
                }
                teacherResults.Add(new
                {
                    org.OrgId,
                    org.OrgName,
                    Room = roomList,
                    Level = levelList
                });
            }
            result.Data = teacherResults;

            return Ok(result);
        }

        // GET: api/TeacherFilter
        [HttpGet("sessionEditFilter/{courseId}")]
        public async Task<IActionResult> sessionEditFilterGet(int courseId)
        {
            Result<List<Object>> result = new Result<List<Object>>();
            dynamic orgs;
            dynamic rooms;
            dynamic teachers;
            try
            {
                orgs = await (from o in _ablemusicContext.Org
                              select new
                              {
                                  o.OrgId,
                                  o.OrgName,
                              }).ToListAsync();
                rooms = await (from r in _ablemusicContext.Room
                               select new
                               {
                                   r.RoomId,
                                   r.RoomName,
                                   r.OrgId
                               }).ToListAsync();
                teachers = await (from t in _ablemusicContext.Teacher
                                  join ta in _ablemusicContext.AvailableDays on t.TeacherId equals ta.TeacherId
                                  join tc in _ablemusicContext.TeacherCourse on t.TeacherId equals tc.TeacherId
                                  where tc.CourseId == courseId
                                  select new
                                  {
                                      TeacherId = t.TeacherId,
                                      TeacherFirstName = t.FirstName,
                                      TeacherLastName = t.LastName,
                                      TeacherAvailableOrgId = ta.OrgId,
                                      TeacherLevelId = t.Level,
                                  } into x
                                  group x by new
                                  {
                                      x.TeacherId,
                                      x.TeacherAvailableOrgId
                                  } into g
                                  select g.FirstOrDefault()
                                  ).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            List<Object> teacherResults = new List<Object>();
            foreach (var org in orgs)
            {
                List<Object> levelList = new List<Object>();
                List<Object> teacherList = new List<Object>();
                foreach (var teacher in teachers)
                {
                    if (teacher.TeacherAvailableOrgId == org.OrgId)
                    {
                        teacherList.Add(new
                        {
                            teacher.TeacherId,
                            TeacherName = teacher.TeacherFirstName + " " + teacher.TeacherLastName,
                        });
                    }
                }

                List<Object> roomList = new List<object>();
                foreach (var room in rooms)
                {
                    if (org.OrgId == room.OrgId)
                    {
                        roomList.Add(new
                        {
                            room.RoomId,
                            room.RoomName
                        });
                    }
                }
                teacherResults.Add(new
                {
                    org.OrgId,
                    org.OrgName,
                    Room = roomList,
                    Teacher = teacherList,
                });
            }
            result.Data = teacherResults;

            return Ok(result);
        }
    }
}
