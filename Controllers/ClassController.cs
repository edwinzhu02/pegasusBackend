using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        
        public ClassController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        //GET: http://localhost:5000/api/class/groupCourse
        [HttpGet]
        [Route("groupCourse")]
        public ActionResult<List<GroupCourseInstance>> GetGroupClassDetails()
        {
            return _pegasusContext.GroupCourseInstance.
                Include(s=>s.Course).
                Include(s=>s.Org)
                .ToList();
        }
        
        //GET: http://localhost:5000/api/class/OnetoOneCourse
        [HttpGet]
        [Route("OnetoOneCourse")]
        public ActionResult<List<One2oneCourseInstance>> GetOnetoOneClassDetails()
        {
            return _pegasusContext.One2oneCourseInstance.
                Include(s => s.Course).
                Include(s=>s.Org).
                ToList();
        }
        
        
    }
}