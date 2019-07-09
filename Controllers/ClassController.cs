using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController: ControllerBase
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private List<GroupCourseInstance> details;
        
        public ClassController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        //GET: http://localhost:5000/api/class/groupCourse
        [HttpGet]
        [Route("groupCourse")]
        public ActionResult GetGroupClassDetails()
        {
            
            Result<List<GroupCourseInstance>> result = new Result<List<GroupCourseInstance>>();
            try
            {
                var a = await _
            }
            
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = details;
            return Ok(result);
        }
        
        
        
    }
}