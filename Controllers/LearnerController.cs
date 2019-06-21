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

        [HttpGet]
        [Route("two")]
        public async Task<IActionResult> GetLearner2()
        {
            var result = new Result<Object>();
            try
            {
                var learner = _pegasusContext.Learner
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
                    .Include(w=>w.LearnerGroupCourse)
                    .ThenInclude(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.Course)
                    .Include(s=>s.LearnerGroupCourse)
                    .ThenInclude(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.Room)
                    .Where(s=>s.IsActive ==1)
                    .Select(s=> new
                    {
                        s.LearnerId,s.FirstName,s.LastName,s.EnrollDate,s.ContactNum,s.Email,s.Address,s.IsUnder18,s.Dob,
                        s.Gender,s.IsAbrsmG5,s.G5Certification,s.CreatedAt,s.ReferrerLearnerId,s.Photo,s.Note,s.MiddleName,s.LevelType,
                        s.LearnerLevel,s.PaymentPeriod,s.Referrer,s.FormUrl,s.OtherfileUrl,
                        Parent= s.Parent.Select(w=>new{w.FirstName,w.LastName,w.Email,w.ContactNum,w.Relationship,w.CreatedAt}),
                        LearnerOthers=s.LearnerOthers.Select(w=>new{w.OthersType,w.OthersValue,w.LearnerOthersId,w.LearnerLevel}),
                        One2oneCourseInstance=s.One2oneCourseInstance.Select(w=>new
                        {
                            w.BeginDate,w.EndDate,w.InvoiceDate,
                            Org=new{w.Org.OrgId,w.Org.OrgName},
                            Course=new
                            {
                                w.Course.CourseId,w.Course.CourseName,w.Course.CourseType,w.Course.Level,
                                w.Course.Duration,w.Course.Price,w.Course.CourseCategoryId,w.Course.TeacherLevel
                            },
                            Room=new{w.Room.RoomId,w.Room.RoomName},
                            CourseSchedule=w.CourseSchedule.Select(q=>new{q.CourseInstanceId,q.DayOfWeek,q.CourseScheduleId,q.BeginTime,q.EndTime}),
                            //下面有问题
                            Teacher=new
                            {
                                w.Teacher.TeacherId,w.Teacher.FirstName,w.Teacher.LastName,
                            }
                        }),
                        LearnerGroupCourse= s.LearnerGroupCourse.Select(w=>new
                        {
                            w.LearnerGroupCourseId, w.LearnerId,w.GroupCourseInstanceId,w.CreatedAt,w.IsActivate,
                            w.Comment,w.BeginDate,w.EndDate,
                            GroupCourseInstance = new
                            {
                                w.GroupCourseInstance.CourseId,w.GroupCourseInstance.TeacherId,w.GroupCourseInstance.BeginDate,
                                w.GroupCourseInstance.EndDate,w.GroupCourseInstance.RoomId,w.GroupCourseInstance.OrgId,
                                w.GroupCourseInstanceId,w.GroupCourseInstance.IsActivate,w.GroupCourseInstance.IsStarted,
                                Teacher=new
                                {
                                    w.GroupCourseInstance.Teacher.TeacherId,w.GroupCourseInstance.Teacher.FirstName,w.GroupCourseInstance.Teacher.LastName
                                },
                                CourseSchedule=w.GroupCourseInstance.CourseSchedule.Select(q=>new{q.GroupCourseInstanceId,q.DayOfWeek,q.CourseScheduleId,q.BeginTime,q.EndTime}),
                                Course= new
                                {
                                    w.GroupCourseInstance.CourseId, w.GroupCourseInstance.Course.CourseName,
                                    w.GroupCourseInstance.Course.CourseType, w.GroupCourseInstance.Course.Level,
                                    w.GroupCourseInstance.Course.Duration, w.GroupCourseInstance.Course.Price,
                                    w.GroupCourseInstance.Course.TeacherLevel
                                },
                                Room = new
                                {
                                    w.GroupCourseInstance.Room.RoomId,w.GroupCourseInstance.Room.OrgId,
                                    w.GroupCourseInstance.Room.RoomName,w.GroupCourseInstance.Room.IsActivate
                                }
                                 
                            }
                        })                     
                        
                    })
                    .ToListAsync();
                result.Data = learner;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
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
                    .Include(w=>w.LearnerGroupCourse)
                    .ThenInclude(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.Course)
                    .Include(s=>s.One2oneCourseInstance)
                    .ThenInclude(s=>s.CourseSchedule)
                    .Include(s=>s.LearnerGroupCourse)
                    .ThenInclude(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.Room)
                    .Include(s=>s.Amendment)
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
        
        //PUT api/learner/:id
        [HttpPut("{id}")]
        [CheckModelFilter]
        public async Task<IActionResult> StudentUpdate(int id,[FromForm(Name = "photo")] IFormFile image, [FromForm(Name = "ABRSM")] IFormFile ABRSM,
            [FromForm(Name="Form")] IFormFile Form, [FromForm(Name="Otherfile")] IFormFile Otherfile,[FromForm] string details)
        {
            var result = new Result<string>();
            try
            {
                if (_pegasusContext.Learner.FirstOrDefault(s => s.LearnerId == id) == null)
                {
                    throw new Exception("Learner does not exist");
                }
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<StudentRegister>(details);
                    var learner = _pegasusContext.Learner.FirstOrDefault(s => s.LearnerId == id);
                    
                    //delete parents for this learner
                    _pegasusContext.Parent.Where(s=>s.LearnerId==id).ToList().ForEach(s =>
                        {
                            _pegasusContext.Remove(s);
                        });
                    await _pegasusContext.SaveChangesAsync();
                    
                    //delete learner other
                    _pegasusContext.LearnerOthers.Where(s=>s.LearnerId==id).ToList().ForEach(s =>
                        {
                            _pegasusContext.Remove(s);
                        });
                    await _pegasusContext.SaveChangesAsync();
                    
                    //update learner
                    _mapper.Map(detailsJson, learner);
                    _pegasusContext.Update(learner);
                    await _pegasusContext.SaveChangesAsync();
                    
                    
                    //upload file
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");
                   
                    if (image != null)
                    {
                        learner.Photo = $"images/learner/Photos/{learner.LearnerId+strDateTime+Path.GetExtension(image.FileName)}";
                        _pegasusContext.Update(learner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(image,"learner/Photos/",learner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }
                    
                    if (ABRSM != null)
                    {
                        learner.G5Certification = $"images/learner/ABRSM_Grade5_Certificate/{learner.LearnerId+strDateTime+Path.GetExtension(ABRSM.FileName)}";
                        learner.IsAbrsmG5 = 1;
                        _pegasusContext.Update(learner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(ABRSM,"learner/ABRSM_Grade5_Certificate/",learner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                        
                    }
                    
                    if (Form != null)
                    {
                        learner.FormUrl = $"images/learner/Form/{learner.LearnerId+strDateTime+Path.GetExtension(Form.FileName)}";
                        _pegasusContext.Update(learner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Form,"learner/Form",learner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Otherfile != null)
                    {
                        learner.OtherfileUrl = $"images/learner/Otherfile/{learner.LearnerId+strDateTime+Path.GetExtension(Otherfile.FileName)}";
                        _pegasusContext.Update(learner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Otherfile,"learner/Otherfile",learner.LearnerId,strDateTime);
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
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        }
        
        //POST: http://localhost:5000/api/learner
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> StudentRegister([FromForm(Name = "photo")] IFormFile image, [FromForm(Name = "ABRSM")] IFormFile ABRSM,
            [FromForm(Name="Form")] IFormFile Form, [FromForm(Name="Otherfile")] IFormFile Otherfile
            ,[FromForm] string details)
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
                    newLearner.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    _pegasusContext.Add(newLearner);
                    await _pegasusContext.SaveChangesAsync();
                    
                    newLearner.LearnerGroupCourse.ToList().ForEach(s =>
                    {
                        s.CreatedAt=toNZTimezone(DateTime.UtcNow);
                        s.IsActivate = 1;
                        _pegasusContext.Update(s);
                    });

                    await _pegasusContext.SaveChangesAsync();

                    for (var i = 0; i < newLearner.One2oneCourseInstance.ToList().Count; i++)
                    {
                        detailsJson.One2oneCourseInstance[i].id =
                            newLearner.One2oneCourseInstance.ToList()[i].CourseInstanceId;
                    }
                    detailsJson.One2oneCourseInstance.ForEach(s =>
                    {
                        if (_pegasusContext.Course.FirstOrDefault(w => w.CourseId == s.CourseId).CourseType != 1)
                        {
                            throw new Exception("This course is not one to one course");
                        }
                        var durationType = _pegasusContext.Course.FirstOrDefault(w => w.CourseId == s.CourseId).Duration;
                        _pegasusContext.Add(new CourseSchedule
                        {
                            DayOfWeek = s.Schedule.DayOfWeek,CourseInstanceId = s.id,
                            BeginTime = s.Schedule.BeginTime, 
                            EndTime = GetEndTimeForOnetoOneCourseSchedule(s.Schedule.BeginTime,durationType)
                        });
                    });
                    await _pegasusContext.SaveChangesAsync();
                    
                    //add new fund row for new student
                    var fundItem = new Fund{Balance = 0,LearnerId = newLearner.LearnerId};
                    _pegasusContext.Add(fundItem);
                    await _pegasusContext.SaveChangesAsync();
                    
                    //add create new user for this learner
                    var newUser = new User
                    {
                        UserName = newLearner.Email,Password = "helloworld",
                        CreatedAt = toNZTimezone(DateTime.UtcNow),RoleId = 4, IsActivate = 1
                        
                    };
                    _pegasusContext.Add(newUser);
                    await _pegasusContext.SaveChangesAsync();
                    
                    //keep userid to learner table
                    newLearner.UserId = newUser.UserId;
                    _pegasusContext.Update(newLearner);
                    await _pegasusContext.SaveChangesAsync();
                    
                    //upload file
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");
                   
                    if (image != null)
                    {
                        newLearner.Photo = $"images/learner/Photos/{newLearner.LearnerId+strDateTime+Path.GetExtension(image.FileName)}";
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(image,"learner/Photos/",newLearner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }
                    
                    if (ABRSM != null)
                    {
                        newLearner.G5Certification = $"images/learner/ABRSM_Grade5_Certificate/{newLearner.LearnerId+strDateTime+Path.GetExtension(ABRSM.FileName)}";
                        newLearner.IsAbrsmG5 = 1;
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(ABRSM,"learner/ABRSM_Grade5_Certificate/",newLearner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                        
                    }
                    
                    if (Form != null)
                    {
                        newLearner.FormUrl = $"images/learner/Form/{newLearner.LearnerId+strDateTime+Path.GetExtension(Form.FileName)}";
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Form,"learner/Form",newLearner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Otherfile != null)
                    {
                        newLearner.OtherfileUrl = $"images/learner/Otherfile/{newLearner.LearnerId+strDateTime+Path.GetExtension(Otherfile.FileName)}";
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Otherfile,"learner/Otherfile",newLearner.LearnerId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
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