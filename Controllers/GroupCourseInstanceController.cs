using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupCourseInstanceController : BasicController
    {
        private readonly IMapper _mapper;

        public GroupCourseInstanceController(ablemusicContext ablemusicContext, ILogger<GroupCourseInstanceController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        // GET: api/groupcourseinstance
        [HttpGet]
        public async Task<IActionResult> GetGroupCourseInstance()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var schedule = _ablemusicContext.CourseSchedule;
                var item = await _ablemusicContext.GroupCourseInstance
                    .Include(c => c.CourseSchedule)
                    .Include(s => s.Course)
                    //.ThenInclude(s => s.CourseCategory)                    
                    .Include(s => s.Org)
                    .Include(s => s.Room)
                    .Include(s => s.Teacher)
                    .Where(s=>s.IsActivate == 1)
                    .Select(s => new
                    {
                        s.CourseId, s.GroupCourseInstanceId, s.BeginDate, s.EndDate,
                        schedule = schedule.Where(w=>w.GroupCourseInstanceId==s.GroupCourseInstanceId),
                        Course = new
                        {
                            s.Course.CourseId, s.Course.CourseName, s.Course.CourseType, s.Course.Level,
                            s.Course.Duration, s.Course.Price ,s.Course.CourseCategory
                        },
                        Org = new {s.Org.OrgId, s.Org.OrgName}, Room = new {s.Room.RoomId, s.Room.RoomName},
                        Teacher = new {s.Teacher.TeacherId, s.Teacher.FirstName, s.Teacher.LastName}
                    })
                    .ToListAsync();
                

                result.Data = item;
                result.IsSuccess = true;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        
        //Delete api/groupcourseinstance
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupCourseInstance(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                var item = _ablemusicContext.GroupCourseInstance.FirstOrDefault(s => s.GroupCourseInstanceId == id);
                if (item == null)
                {
                    result.IsFound = false;
                    throw new Exception("Group course instance does not found");
                }
                item.IsActivate = 0;
                _ablemusicContext.Update(item);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = "success";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        
        //POST: api/groupcourseinstance
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostgroupCourseInstance([FromBody] GroupCourseInstanceModel groupinstance)
        {
            Result<string> result = new Result<string>();
            try
            {
                if (_ablemusicContext.Course.FirstOrDefault(s => s.CourseId == groupinstance.CourseId).CourseType !=2)
                {
                    throw new Exception("This course is not group course");
                }
                var duration = _ablemusicContext.Course.FirstOrDefault(s => s.CourseId == groupinstance.CourseId).Duration;
                for (var i = 0; i < groupinstance.CourseSchedule.Count; i++ )
                {
                    groupinstance.CourseSchedule[i].EndTime =
                        GetEndTimeForOnetoOneCourseSchedule(groupinstance.CourseSchedule[i].BeginTime.Value, duration);
                }
                var newGroupInstance = new GroupCourseInstance();
                _mapper.Map(groupinstance, newGroupInstance);
                newGroupInstance.IsActivate = 1;
                _ablemusicContext.Add(newGroupInstance);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = "success";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        //PUT api/groupcourseinstance/:id
        [HttpPut("{id}")]
        [CheckModelFilter]
        public async Task<IActionResult> UpdateGroupCourseInstance(int id, [FromBody] GroupCourseInstanceModel groupCourseInstanceModel)
        {
            Result<string> result = new Result<string>();
            try
            {
                var groupCourseinstance =
                    _ablemusicContext.GroupCourseInstance.FirstOrDefault(s => s.GroupCourseInstanceId == id);
                if (groupCourseinstance == null)
                {
                    throw new Exception("Group course instance does not found");
                }
                
                if (_ablemusicContext.Course.FirstOrDefault(s => s.CourseId == groupCourseInstanceModel.CourseId).CourseType !=2)
                {
                    throw new Exception("This course is not group course");
                }

                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    
                    var scheduleList = _ablemusicContext.CourseSchedule.Where(s => s.GroupCourseInstanceId == id);
                    scheduleList.ToList().ForEach(s => { _ablemusicContext.Remove(s); });
                    await _ablemusicContext.SaveChangesAsync();
                    var duration = _ablemusicContext.Course.FirstOrDefault(s => s.CourseId == groupCourseInstanceModel.CourseId).Duration;
                    for (var i = 0; i < groupCourseInstanceModel.CourseSchedule.Count; i++ )
                    {
                        groupCourseInstanceModel.CourseSchedule[i].EndTime =
                            GetEndTimeForOnetoOneCourseSchedule(groupCourseInstanceModel.CourseSchedule[i].BeginTime.Value, duration);
                    }
                    _mapper.Map(groupCourseInstanceModel, groupCourseinstance);
                    _ablemusicContext.Update(groupCourseinstance);
                    await _ablemusicContext.SaveChangesAsync();
                    dbContextTransaction.Commit();
                }
                result.Data = "success";
                
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
