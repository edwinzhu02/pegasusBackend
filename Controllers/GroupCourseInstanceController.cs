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
            result.IsSuccess = true;
            result.Data = _pegasusContext.GroupCourseInstance
                .Include(c => c.CourseSchedule)
                .Include(s => s.Course)
                .Include(s => s.Org)
                .Include(s => s.Room)
                .Include(s => s.Teacher)
                .Select(s => new
                {
                    s.CourseId,s.BeginDate,s.EndDate,s.GroupCourseInstanceId,
                    Course = new {s.Course.CourseId,s.Course.CourseName,s.Course.CourseType,s.Course.Level,s.Course.Duration,s.Course.Price},
                    Org = new {s.Org.OrgId, s.Org.OrgName}, Room= new {s.Room.RoomId,s.Room.RoomName},
                    Teacher = new {s.Teacher.TeacherId,s.Teacher.FirstName,s.Teacher.LastName}
                });
                
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