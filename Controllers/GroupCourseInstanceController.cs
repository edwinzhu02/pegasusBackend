using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using AutoMapper;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupCourseInstanceController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private readonly IMapper _mapper;

        public GroupCourseInstanceController(pegasusContext.ablemusicContext pegasusContext, IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }

        // GET: api/groupcourseinstance
        [HttpGet]
        public async Task<IActionResult> GetGroupCourseInstance()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var schedule = _pegasusContext.CourseSchedule;
                var item = await _pegasusContext.GroupCourseInstance
                    .Include(c => c.CourseSchedule)
                    .Include(s => s.Course)
                    .Include(s => s.Org)
                    .Include(s => s.Room)
                    .Include(s => s.Teacher)
                    .Where(s=>s.IsActivate == 1)
                    .Select(s => new
                    {
                        s.CourseId, s.GroupCourseInstanceId, schedule = schedule.Where(w=>w.GroupCourseInstanceId==s.GroupCourseInstanceId),
                        Course = new
                        {
                            s.Course.CourseId, s.Course.CourseName, s.Course.CourseType, s.Course.Level,
                            s.Course.Duration, s.Course.Price
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
                var item = _pegasusContext.GroupCourseInstance.FirstOrDefault(s => s.GroupCourseInstanceId == id);
                if (item == null)
                {
                    result.IsFound = false;
                    throw new Exception("Group course instance does not found");
                }
                item.IsActivate = 0;
                _pegasusContext.Update(item);
                await _pegasusContext.SaveChangesAsync();
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
                var newGroupInstance = new GroupCourseInstance();
                _mapper.Map(groupinstance, newGroupInstance);
                newGroupInstance.IsActivate = 1;
                _pegasusContext.Add(newGroupInstance);
                await _pegasusContext.SaveChangesAsync();
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
                    _pegasusContext.GroupCourseInstance.FirstOrDefault(s => s.GroupCourseInstanceId == id);
                if (groupCourseinstance == null)
                {
                    throw new Exception("Group course instance does not found");
                }

                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var scheduleList = _pegasusContext.CourseSchedule.Where(s => s.GroupCourseInstanceId == id);
                    scheduleList.ToList().ForEach(s => { _pegasusContext.Remove(s); });
                    await _pegasusContext.SaveChangesAsync();
                    _mapper.Map(groupCourseInstanceModel, groupCourseinstance);
                    _pegasusContext.Update(groupCourseinstance);
                    await _pegasusContext.SaveChangesAsync();
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