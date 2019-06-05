using System;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using AutoMapper;
using Pegasus_backend.Services;


namespace Pegasus_backend.Controllers
{
    
    [Route("cr")]
    [ApiController]
    
    public class CourseRemainingController: ControllerBase
    {
        private readonly DataService _dataService;
        private readonly IMapper _mapper;

        public CourseRemainingController(ablemusicContext context, IMapper mapper)
        {
            _dataService = new DataService(context);
            _mapper = mapper;
        }

        [HttpGet("l/{studentId}")]

        public Result<IEnumerable<Lesson>> GetCourse(int studentId)
        {
            return _dataService.GetLessons(studentId).Result;

        }
        
        
    }
}