using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnetoOneCourseInstanceController : BasicController

    {
        private readonly IMapper _mapper;

        public OnetoOneCourseInstanceController(ablemusicContext ablemusicContext, ILogger<OnetoOneCourseInstanceController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        [HttpPut("{instanceId}/{endDate}")]
        public async Task<IActionResult> UpdateOnetoOneCourseInstance(int instanceId, DateTime? endDate)
        {
            var result = new Result<string>();
            try
            {
                var item = _ablemusicContext.One2oneCourseInstance.FirstOrDefault(s => s.CourseInstanceId == instanceId);
                if (item == null)
                {
                    throw new Exception("Course instance does not found");
                }

                item.EndDate = endDate;
                _ablemusicContext.Update(item);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = "success";
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddOnetoOneCourseInstance([FromBody] OnetoOneCourseInstancesModel model)
        {
            var result = new Result<string>();
            try
            {
                using (var dbtransaction =await _ablemusicContext.Database.BeginTransactionAsync())
                {
                    model.OnetoOneCourses.ForEach(q =>
                    {
                        var courseInstance = new One2oneCourseInstance();
                        _mapper.Map(q, courseInstance);
                        if (_ablemusicContext.Course.FirstOrDefault(s => courseInstance.CourseId == s.CourseId).CourseType != 1)
                        {
                            throw new Exception("This course is not one to one course");
                        }

                        var durationType = _ablemusicContext.Course.FirstOrDefault(s => s.CourseId == courseInstance.CourseId)
                            .Duration;
                        _ablemusicContext.Add(courseInstance);
                        _ablemusicContext.SaveChanges();

                        var schedule = new CourseSchedule
                        {
                            DayOfWeek = q.Schedule.DayOfWeek, CourseInstanceId = courseInstance.CourseInstanceId,
                            BeginTime = q.Schedule.BeginTime,
                            EndTime = GetEndTimeForOnetoOneCourseSchedule(q.Schedule.BeginTime,durationType)
                        };
                        _ablemusicContext.Add(schedule);
                        _ablemusicContext.SaveChanges();
                    });
                    
                    dbtransaction.Commit();
                }

                result.Data = "success";
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