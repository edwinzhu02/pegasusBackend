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
                    model.OnetoOneCourses.ForEach(s =>
                    {
                        var room = _ablemusicContext.AvailableDays.FirstOrDefault(
                            q => q.TeacherId == s.TeacherId && q.OrgId == s.OrgId &&
                                 q.DayOfWeek == s.Schedule.DayOfWeek);
                            

                        if (room == null)
                        {
                            throw new Exception("Room does not found");
                        }
                        
                        var durationType = _ablemusicContext.Course.FirstOrDefault(w => w.CourseId == s.CourseId).Duration;
                        _ablemusicContext.Add(new One2oneCourseInstance
                        {
                            CourseId = s.CourseId,TeacherId = s.TeacherId, OrgId = s.OrgId,
                            BeginDate = s.BeginDate, EndDate = s.EndDate, LearnerId = s.LearnerId,
                            RoomId = room.RoomId,
                            CourseSchedule = new List<CourseSchedule>()
                            {
                                new CourseSchedule
                                {
                                    DayOfWeek = s.Schedule.DayOfWeek,
                                    BeginTime = s.Schedule.BeginTime, 
                                    EndTime = GetEndTimeForOnetoOneCourseSchedule(s.Schedule.BeginTime,durationType)
                                }
                            }
                        });
                    });
                    await _ablemusicContext.SaveChangesAsync();
                        
                
                    
                    dbtransaction.Commit();
                }

                result.Data = "success";
                return Ok(result);


            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        }
    }
}