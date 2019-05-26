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
    public class LearnerController: BasicController
    {
        
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private readonly IMapper _mapper;
        public LearnerController(pegasusContext.ablemusicContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        //Delete: api/learner/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearner(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                var learner = await _pegasusContext.Learner.FirstOrDefaultAsync(s => s.LearnerId == id);
                if (learner == null)
                {
                    throw new Exception("Learner does not exist");
                }
                learner.IsActive = 0;
                _pegasusContext.Update(learner);
                await _pegasusContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = "delete successfully";
            return Ok(result);
        }
        
        //GET: http://localhost:5000/api/learner/:name
        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> GetLearner(string name)
        {
            Result<IEnumerable<Learner>> result = new Result<IEnumerable<Learner>>();
            try
            {
                result.Data = await _pegasusContext.Learner
                    .Include(s=>s.LearnerOthers)
                    .Include(s=>s.Parent)
                    .Include(s=>s.One2oneCourseInstance)
                    .Include(s=>s.LearnerGroupCourse)
                    .Where(s=>s.IsActive==1 &&s.FirstName.Contains(name))
                    .ToListAsync();
                if (result.Data.Count() == 0)
                {
                    return NotFound(DataNotFound(result));
                }
                

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);


        }
        
        //GET: http://localhost:5000/api/learner
        [HttpGet]
        public async Task<ActionResult<List<Learner>>> GetLearners()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var data = await _pegasusContext.Learner
                    .Include(w=>w.Parent)
                    .Include(w=>w.LearnerOthers)
                    .Include(w=>w.One2oneCourseInstance)
                    .ThenInclude(w=>w.Org)
                    .Include(w=>w.One2oneCourseInstance)
                    .ThenInclude(w=>w.Course)
                    .Include(w=>w.One2oneCourseInstance)
                    .ThenInclude(w=>w.Room)
                    .Include(w=>w.One2oneCourseInstance)
                    .ThenInclude(w=>w.CourseSchedule)
                    .Include(w=>w.One2oneCourseInstance)
                    .ThenInclude(w=>w.Teacher)
                    .Include(w=>w.LearnerGroupCourse)
                    .ThenInclude(w=>w.GroupCourseInstance)
                    .ThenInclude(s=>s.Teacher)
                    .Include(w=>w.LearnerGroupCourse)
                    .ThenInclude(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.CourseSchedule)
                    .Include(s=>s.One2oneCourseInstance)
                    .ThenInclude(s=>s.CourseSchedule)
                    .Where(s=>s.IsActive ==1)
                    
                    .ToListAsync();
                result.Data = data;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        
        //POST: http://localhost:5000/api/learner
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> StudentRegister([FromForm(Name = "photo")] IFormFile image, [FromForm(Name = "ABRSM")] IFormFile ABRSM,[FromForm] string details)
        {
            Result<string> result = new Result<string>();
            try
            {
                
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<StudentRegister>(details);
                    var newLearner = new Learner();
                    _mapper.Map(detailsJson, newLearner);
                    newLearner.IsActive = 1;
                    _pegasusContext.Add(newLearner);
                    await _pegasusContext.SaveChangesAsync();

                    for (var i = 0; i < newLearner.One2oneCourseInstance.ToList().Count; i++)
                    {
                        detailsJson.One2oneCourseInstance[i].id =
                            newLearner.One2oneCourseInstance.ToList()[i].CourseInstanceId;
                    }
                    detailsJson.One2oneCourseInstance.ForEach(s =>
                    {
                        _pegasusContext.Add(new CourseSchedule
                        {
                            DayOfWeek = s.Schedule.DayOfWeek,CourseInstanceId = s.id,
                            BeginTime = s.Schedule.BeginTime, EndTime = GetEndTimeForOnetoOneCourseSchedule(s.Schedule.BeginTime,s.Schedule.DurationType)
                        });
                    });
                    await _pegasusContext.SaveChangesAsync();
                    
                    if (image != null)
                    {
                        newLearner.Photo = $"images/LearnerImages/{newLearner.LearnerId+Path.GetExtension(image.FileName)}";
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(image,"image",newLearner.LearnerId);
                    }

                    if (ABRSM != null)
                    {
                        newLearner.G5Certification = $"images/ABRSM_Grade5_Certificate/{newLearner.LearnerId+Path.GetExtension(ABRSM.FileName)}";
                        newLearner.IsAbrsmG5 = 1;
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(ABRSM,"ABRSM",newLearner.LearnerId);
                    }
                    
                    
                    dbContextTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            
            result.Data = "success!";
            return Ok(result);
        }
    }
}