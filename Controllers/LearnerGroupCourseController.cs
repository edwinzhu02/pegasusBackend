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
using Pegasus_backend.Controllers;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Services;
using AutoMapper;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerGroupCourseController: BasicController
    {
        private readonly IMapper _mapper;
        private readonly ILogger<LearnerGroupCourseController> _logger;
        private readonly LessonGenerateService _lessonGenerateService;
        private readonly GroupCourseGenerateService _groupCourseGenerateService;

        public LearnerGroupCourseController(ablemusicContext ablemusicContext, ILogger<LearnerGroupCourseController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
            _logger = log;
            _lessonGenerateService = new LessonGenerateService(_ablemusicContext, _mapper);
        }

        [HttpPost]
        public async Task<IActionResult> AddLearnerGroupCourseController([FromBody] LearnerGroupCourseModel model)
        {
            var result = new Result<string>();
            try
            {
                model.LearnerGroupCourses.ForEach(s => {
                    if (_ablemusicContext.LearnerGroupCourse.FirstOrDefault(w => w.LearnerId == s.LearnerId && w.GroupCourseInstanceId==s.GroupCourseInstanceId) != null)
                    {
                        throw new Exception("Learner has joined this group course");
                    }
                    var item = new LearnerGroupCourse
                    {
                        LearnerId = s.LearnerId, GroupCourseInstanceId = s.GroupCourseInstanceId,
                        Comment = s.Comment, BeginDate = s.BeginDate, CreatedAt = toNZTimezone(DateTime.UtcNow),
                        IsActivate = 1
                    };
                    _ablemusicContext.Add(item);
                });
                await _ablemusicContext.SaveChangesAsync();
                model.LearnerGroupCourses.ForEach(async s =>
                {
                    await _lessonGenerateService.GetTerm((DateTime)s.BeginDate, (int)s.GroupCourseInstanceId, 0);
                });
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

        [HttpPut("{learnerGroupCourseId}/{endDate}")]
        public async Task<IActionResult> UpdateLearnerGroupCourseController(int learnerGroupCourseId, DateTime? endDate)
        {
            var result = new Result<string>();
            try
            {
                var item = _ablemusicContext.LearnerGroupCourse.FirstOrDefault(s =>
                    s.LearnerGroupCourseId == learnerGroupCourseId);
                if (item == null)
                {
                    return NotFound(DataNotFound(result));
                }
                item.EndDate = endDate;
                _ablemusicContext.Update(item);
                await _ablemusicContext.SaveChangesAsync();
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
        [HttpPost("termId")]
        public async Task<IActionResult> TestGroupCourseGenerate(short termId)
        {
            var _groupCourseGenerateService = new GroupCourseGenerateService(_ablemusicContext, _logger);
            var result =await  _groupCourseGenerateService.GenerateLessons(termId);
             return Ok(result);
        }


    }
}