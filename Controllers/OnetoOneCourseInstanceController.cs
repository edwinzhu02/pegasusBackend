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
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnetoOneCourseInstanceController : BasicController

    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private readonly IMapper _mapper;

        public OnetoOneCourseInstanceController(pegasusContext.ablemusicContext pegasusContext, IMapper mapper)
        {
            _mapper = mapper;
            _pegasusContext = pegasusContext;
        }

        [HttpPut("{instanceId}/{endDate}")]
        public async Task<IActionResult> UpdateOnetoOneCourseInstance(int instanceId, DateTime? endDate)
        {
            var result = new Result<string>();
            try
            {
                var item = _pegasusContext.One2oneCourseInstance.FirstOrDefault(s => s.CourseInstanceId == instanceId);
                if (item == null)
                {
                    throw new Exception("Course instance does not found");
                }

                item.EndDate = endDate;
                _pegasusContext.Update(item);
                await _pegasusContext.SaveChangesAsync();
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
                using (var dbtransaction =await _pegasusContext.Database.BeginTransactionAsync())
                {
                    model.OnetoOneCourses.ForEach(q =>
                    {
                        var courseInstance = new One2oneCourseInstance();
                        _mapper.Map(q, courseInstance);
                        if (_pegasusContext.Course.FirstOrDefault(s => courseInstance.CourseId == s.CourseId).CourseType != 1)
                        {
                            throw new Exception("This course is not one to one course");
                        }

                        var durationType = _pegasusContext.Course.FirstOrDefault(s => s.CourseId == courseInstance.CourseId)
                            .Duration;
                        _pegasusContext.Add(courseInstance);
                        _pegasusContext.SaveChanges();

                        var schedule = new CourseSchedule
                        {
                            DayOfWeek = q.Schedule.DayOfWeek, CourseInstanceId = courseInstance.CourseInstanceId,
                            BeginTime = q.Schedule.BeginTime,
                            EndTime = GetEndTimeForOnetoOneCourseSchedule(q.Schedule.BeginTime,durationType)
                        };
                        _pegasusContext.Add(schedule);
                        _pegasusContext.SaveChanges();
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