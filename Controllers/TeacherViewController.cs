using Pegasus_backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNetCore.Http;
//using System.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherViewController:BasicController
    {
        private readonly DataService _service;
        public TeacherViewController(ablemusicContext ablemusicContext, IMapper mapper, ILogger<TeacherViewController> log) : base(ablemusicContext, log)
        {
            _service = new DataService(_ablemusicContext,mapper);
        }



        [HttpGet("TeacherId={id}&LessonDate={date}")]
        public Result<IEnumerable<Lesson>> GetLessonsByTeacherIdAndDate(int id,DateTime date)
        {
            return _service.FilterLessonByDate(_service.GetLessonByTeacher(id), date);

        }


        [HttpGet("hours/TeacherId={id}&LessonDate={date}")]
        public Result<Double> GetHours(int id,DateTime date)
        {
           return  _service.GetHours(GetLessonsByTeacherIdAndDate(id,date));
        }
        

        [HttpGet("isHoursReached/TeacherId={id}&LessonDate={date}")]
        public Result<bool> IsMinHoursReached(int id, DateTime date)
        {
           return _service.IsMinimumHoursReached(id, GetLessonsByTeacherIdAndDate(id, date));
        }

        [HttpGet("hoursDiff/TeacherId={id}&LessonDate={date}")]
        public Result<double> GetHoursDiff(int id, DateTime date)
        {

            return _service.GetHoursDiff(id, date);
        }

        [HttpGet("teacher/{id}")]
        public Result<Teacher> GetTeacher(int id)
        {
            return _service.GetTeacherById(id);
        }
    }
}