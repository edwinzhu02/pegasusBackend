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
    
    [Route("api/[controller]")]
    [ApiController]
    
    public class CourseRemainingController: ControllerBase
    {
        private readonly DataService _dataService;
        private readonly IMapper _mapper;

        public CourseRemainingController(ablemusicContext context, IMapper mapper)
        {
            _dataService = new DataService(context,mapper);
            
        }

        [HttpGet("{studentId}")]

        public Result<IEnumerable<CourseRemain>> GetRemainLessons(int studentId)
        {
            var uc = _dataService.GetUnconfirmedLessons(studentId);
            var lr = _dataService.GetRemainLesson(studentId);
            var result = new Result<IEnumerable<CourseRemain>>();
            if (!uc.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = uc.ErrorMessage;
                return result;

            }
            if (!lr.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = lr.ErrorMessage;
                return result;
            }
            return _dataService.CalculateQuantity(uc.Data,lr.Data);
             
        }

        [HttpGet("test")]
        public Result<IEnumerable<CourseRemain>> test()
        {
            return _dataService.GetRemainLesson(10070);
        }






    }
}