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
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerController : BasicController
    {

        private readonly IMapper _mapper;
        private readonly LessonGenerateService _lessonGenerateService;

        public LearnerController(ablemusicContext ablemusicContext, ILogger<LearnerController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
            _lessonGenerateService = new LessonGenerateService(_ablemusicContext, _mapper);
        }

        //Delete: api/learner/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearner(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                var learner = await _ablemusicContext.Learner.FirstOrDefaultAsync(s => s.LearnerId == id);
                if (learner == null)
                {
                    throw new Exception("Learner does not exist");
                }
                learner.IsActive = 0;
                _ablemusicContext.Update(learner);
                await _ablemusicContext.SaveChangesAsync();
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

        [HttpGet]
        public async Task<IActionResult> GetLearner()
        {
            var result = new Result<object>();
            try
            {
                var learners = await _ablemusicContext.Learner
                    .Include(w => w.Parent)
                    .Include(w => w.LearnerOthers)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Amendment)
                    .ThenInclude(w => w.Room)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Amendment)
                    .ThenInclude(w => w.Teacher)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Amendment)
                    .ThenInclude(w => w.Org)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Org)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Room)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.CourseSchedule)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Teacher)
                    .Include(w => w.LearnerGroupCourse)
                    .ThenInclude(w => w.GroupCourseInstance)
                    .ThenInclude(s => s.Teacher)
                    .Include(w => w.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.CourseSchedule)
                    .Include(w => w.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.Course)
                    .Include(s => s.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.Room)//

                    .Where(s => s.IsActive == 1).OrderBy(s=>s.FirstName)
                    .ToListAsync();

                var mapperItem = _mapper.Map<List<GetLearnerModel>>(learners);
                result.Data = mapperItem;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpGet]
        [Route("[action]/{learnerId}")]
        public async Task<IActionResult> GetLearnerById(int learnerId)
        {
            var result = new Result<object>();
            try
            {
                var learners = await _ablemusicContext.Learner
                    .Include(w => w.Parent)
                    .Include(w => w.LearnerOthers)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Amendment)
                    .ThenInclude(w => w.Room)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Amendment)
                    .ThenInclude(w => w.Teacher)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Org)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Room)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.CourseSchedule)
                    .Include(w => w.One2oneCourseInstance)
                    .ThenInclude(w => w.Teacher)
                    .Include(w => w.LearnerGroupCourse)
                    .ThenInclude(w => w.GroupCourseInstance)
                    .ThenInclude(s => s.Teacher)
                    .Include(w => w.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.CourseSchedule)
                    .Include(w => w.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.Course)
                    .Include(s => s.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.Room)//
                    .Where(s => s.LearnerId == learnerId)
                    .FirstOrDefaultAsync();

                var mapperItem = _mapper.Map<GetLearnerModel>(learners);
                result.Data = mapperItem;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpGet]
        [Route("[action]/{learnerId}")]
        public async Task<IActionResult> GetOrgByLearner(int learnerId)
        {
            var result = new Result<object>();
            try{
                 var learners = await _ablemusicContext.Learner
                    .Include(w => w.Org)
                    .FirstOrDefaultAsync(l => l.LearnerId == learnerId);
                learners.Org.Learner=null;
                result.Data = learners.Org;  
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        //GET: http://localhost:5000/api/learner/:name
        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> GetLearner(string name)
        {
            Result<IEnumerable<Learner>> result = new Result<IEnumerable<Learner>>();
            try
            {
                result.Data = await _ablemusicContext.Learner
                    .Include(s => s.LearnerOthers)
                    .Include(s => s.Parent)
                    .Include(s => s.One2oneCourseInstance)
                    .Include(s => s.LearnerGroupCourse)
                    .Where(s => s.IsActive == 1 && s.FirstName.Contains(name))
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

        //PUT api/learner/:id
        [HttpPut("{id}")]
        [CheckModelFilter]
        public async Task<IActionResult> StudentUpdate(int id, [FromForm(Name = "photo")] IFormFile image, [FromForm(Name = "ABRSM")] IFormFile ABRSM,
            [FromForm(Name = "Form")] IFormFile Form, [FromForm(Name = "Otherfile")] IFormFile Otherfile, [FromForm] string details)
        {
            var result = new Result<string>();
            try
            {
                if (_ablemusicContext.Learner.FirstOrDefault(s => s.LearnerId == id) == null)
                {
                    throw new Exception("Learner does not exist");
                }
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<StudentUpdate>(details);
                    var learner = _ablemusicContext.Learner.FirstOrDefault(s => s.LearnerId == id);

                    //delete parents for this learner
                    _ablemusicContext.Parent.Where(s => s.LearnerId == id).ToList().ForEach(s =>
                            {
                                _ablemusicContext.Remove(s);
                            });
                    await _ablemusicContext.SaveChangesAsync();

                    //delete learner other
                    _ablemusicContext.LearnerOthers.Where(s => s.LearnerId == id).ToList().ForEach(s =>
                            {
                                _ablemusicContext.Remove(s);
                            });
                    await _ablemusicContext.SaveChangesAsync();

                    //update learner
                    _mapper.Map(detailsJson, learner);
                    _ablemusicContext.Update(learner);
                    await _ablemusicContext.SaveChangesAsync();


                    //upload file
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");

                    if (image != null)
                    {
                        learner.Photo = $"images/learner/Photos/{learner.LearnerId + strDateTime + Path.GetExtension(image.FileName)}";
                        _ablemusicContext.Update(learner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(image, "learner/Photos/", learner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (ABRSM != null)
                    {
                        learner.G5Certification = $"images/learner/ABRSM_Grade5_Certificate/{learner.LearnerId + strDateTime + Path.GetExtension(ABRSM.FileName)}";
                        learner.IsAbrsmG5 = 1;
                        _ablemusicContext.Update(learner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(ABRSM, "learner/ABRSM_Grade5_Certificate/", learner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }

                    }

                    if (Form != null)
                    {
                        learner.FormUrl = $"images/learner/Form/{learner.LearnerId + strDateTime + Path.GetExtension(Form.FileName)}";
                        _ablemusicContext.Update(learner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Form, "learner/Form", learner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Otherfile != null)
                    {
                        learner.OtherfileUrl = $"images/learner/Otherfile/{learner.LearnerId + strDateTime + Path.GetExtension(Otherfile.FileName)}";
                        _ablemusicContext.Update(learner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Otherfile, "learner/Otherfile", learner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }


                    dbContextTransaction.Commit();
                    result.Data = "success";
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        //POST: http://localhost:5000/api/learner
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> StudentRegister([FromForm(Name = "photo")] IFormFile image, [FromForm(Name = "ABRSM")] IFormFile ABRSM,
            [FromForm(Name = "Form")] IFormFile Form, [FromForm(Name = "Otherfile")] IFormFile Otherfile
            , [FromForm] string details)
        {
            Result<object> result = new Result<object>();
            try
            {

                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var all_terms = await _ablemusicContext.Term.Select(x => new { x.TermId, x.BeginDate, x.EndDate }).ToListAsync();
                    var detailsJson = JsonConvert.DeserializeObject<StudentRegister>(details);
                    var newLearner = new Learner();
                    _mapper.Map(detailsJson, newLearner);
                    newLearner.IsActive = 1;
                    newLearner.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    _ablemusicContext.Add(newLearner);
                    await _ablemusicContext.SaveChangesAsync();

                    newLearner.LearnerGroupCourse.ToList().ForEach(s =>
                    {
                        s.CreatedAt = toNZTimezone(DateTime.UtcNow);
                        s.IsActivate = 1;
                        _ablemusicContext.Update(s);
                    });

                    await _ablemusicContext.SaveChangesAsync();

                    //generate new waiting invoice and group lesson

                    newLearner.LearnerGroupCourse.ToList().ForEach(async s =>
                    {
                        await _lessonGenerateService.GetTerm((DateTime)s.BeginDate, (int)s.GroupCourseInstanceId, 0);
                    });
                    if (detailsJson.OneToOneCourseInstance != null)
                    {
                        detailsJson.OneToOneCourseInstance.ForEach(s =>
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
                                CourseId = s.CourseId,
                                TeacherId = s.TeacherId,
                                OrgId = s.OrgId,
                                BeginDate = s.BeginDate,
                                EndDate = s.EndDate,
                                LearnerId = newLearner.LearnerId,
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
                    }
                    await _ablemusicContext.SaveChangesAsync();

                    //generate new waiting invoice and one2one lesson
                    newLearner.One2oneCourseInstance.ToList().ForEach(async s =>
                    {
                        await _lessonGenerateService.GetTerm((DateTime)s.BeginDate, s.CourseInstanceId, 1);
                    });

                    //add new fund row for new student
                    var fundItem = new Fund { Balance = 0, LearnerId = newLearner.LearnerId };
                    _ablemusicContext.Add(fundItem);
                    await _ablemusicContext.SaveChangesAsync();

                    //add create new user for this learner
                    var newUser = new User
                    {
                        UserName = "s"+newLearner.LearnerId.ToString(),
                        Password = "helloworld",
                        CreatedAt = toNZTimezone(DateTime.UtcNow),
                        RoleId = 4,
                        IsActivate = 1

                    };
                    _ablemusicContext.Add(newUser);
                    await _ablemusicContext.SaveChangesAsync();

                    //keep userid to learner table
                    newLearner.UserId = newUser.UserId;
                    _ablemusicContext.Update(newLearner);
                    await _ablemusicContext.SaveChangesAsync();

                    //upload file
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");

                    if (image != null)
                    {
                        newLearner.Photo = $"images/learner/Photos/{newLearner.LearnerId + strDateTime + Path.GetExtension(image.FileName)}";
                        _ablemusicContext.Update(newLearner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(image, "learner/Photos/", newLearner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (ABRSM != null)
                    {
                        newLearner.G5Certification = $"images/learner/ABRSM_Grade5_Certificate/{newLearner.LearnerId + strDateTime + Path.GetExtension(ABRSM.FileName)}";
                        newLearner.IsAbrsmG5 = 1;
                        _ablemusicContext.Update(newLearner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(ABRSM, "learner/ABRSM_Grade5_Certificate/", newLearner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }

                    }

                    if (Form != null)
                    {
                        newLearner.FormUrl = $"images/learner/Form/{newLearner.LearnerId + strDateTime + Path.GetExtension(Form.FileName)}";
                        _ablemusicContext.Update(newLearner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Form, "learner/Form", newLearner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Otherfile != null)
                    {
                        newLearner.OtherfileUrl = $"images/learner/Otherfile/{newLearner.LearnerId + strDateTime + Path.GetExtension(Otherfile.FileName)}";
                        _ablemusicContext.Update(newLearner);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Otherfile, "learner/Otherfile", newLearner.LearnerId, strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }


                    dbContextTransaction.Commit();
                    
                    result.Data = new {LearnerId=newLearner.LearnerId};
                    return Ok(result);
                }
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