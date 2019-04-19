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
        
        public ClassController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        //GET: http://localhost:5000/api/class/groupCourse
        [HttpGet]
        [Route("groupCourse")]
        public ActionResult GetGroupClassDetails()
        {
            return Ok(_pegasusContext.GroupCourseInstance.
                Include(s=>s.Course).
                Include(s=>s.Org)
                .ToList()); 

            /*var data = _pegasusContext.GroupCourseInstance
                .Include(s => s.Course)
                .Include(s => s.Org)
                .Include(s => s.Teacher)
                .Select(n => new
                {
                    n.GroupCourseInstanceId, n.BeginDate, n.EndDate, n.BeginTime, n.EndTime,n.CourseId,
                    n.Course.CourseName,n.OrgId, n.Org.OrgName,n.TeacherId, n.Teacher.FirstName, 
                })
                ;
            return Ok(data);*/
        }
        
        
    }
}