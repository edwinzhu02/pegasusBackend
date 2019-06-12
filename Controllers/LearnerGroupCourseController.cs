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
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerGroupCourseController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;

        public LearnerGroupCourseController(pegasusContext.ablemusicContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddLearnerGroupCourseController([FromBody] LearnerGroupCourseModel model)
        {
            var result = new Result<string>();
            try
            {
                model.LearnerGroupCourses.ForEach(s => {
                    if (_pegasusContext.LearnerGroupCourse.FirstOrDefault(w => w.LearnerId == s.LearnerId && w.GroupCourseInstanceId==s.GroupCourseInstanceId) != null)
                    {
                        throw new Exception("Learner has joined this group course");
                    }
                    var item = new LearnerGroupCourse
                    {
                        LearnerId = s.LearnerId, GroupCourseInstanceId = s.GroupCourseInstanceId,
                        Comment = s.Comment, BeginDate = s.BeginDate, CreatedAt = DateTime.Now,
                        IsActivate = 1
                    };
                    _pegasusContext.Add(item);
                });
                await _pegasusContext.SaveChangesAsync();

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
                var item = _pegasusContext.LearnerGroupCourse.FirstOrDefault(s =>
                    s.LearnerGroupCourseId == learnerGroupCourseId);
                if (item == null)
                {
                    return NotFound(DataNotFound(result));
                }
                item.EndDate = endDate;
                _pegasusContext.Update(item);
                await _pegasusContext.SaveChangesAsync();
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