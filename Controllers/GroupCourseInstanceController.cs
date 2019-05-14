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
    // GET: api/Courses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupCourseInstance>>> GetGroupCourseInstance()
    {
        Result<List<GroupCourseInstance>> result = new Result<List<GroupCourseInstance>>();
        try
        {
            result.IsSuccess = true;
            result.Data = await _pegasusContext.GroupCourseInstance.Include(c => c.CourseSchedule).ToListAsync();
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