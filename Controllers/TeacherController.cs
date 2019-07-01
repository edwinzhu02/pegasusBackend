using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : BasicController
    {
        private readonly IMapper _mapper;
        private TeacherLanguage newTeacherLanguage;
        private TeacherQualificatiion newTeacherQualification;
        private AvailableDays DayList;

        public TeacherController(ablemusicContext ablemusicContext, ILogger<TeacherController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        
        //GET http://localhost:5000/api/teacher/:words
        [CheckModelFilter]
        [HttpGet("{words}")]
        public async Task<IActionResult> SearchTeacher(string words, [FromBody] TeacherSearch searchFormat)
        {
            Result<Object> result = new Result<Object>();
            try
            {
                
                var firstNameList = searchFormat.FirstName?_ablemusicContext.Teacher.Where(s => s.FirstName.Contains(words)).ToList():new List<Teacher>();
                var lastNameList = searchFormat.LastName?_ablemusicContext.Teacher.Where(s => s.LastName.Contains(words)).ToList():new List<Teacher>();
                var genderList = searchFormat.Gender?_ablemusicContext.Teacher.Where(s => s.Gender.ToString().Contains(words)).ToList():new List<Teacher>();
                var mobilePhoneList = searchFormat.MobilePhone?_ablemusicContext.Teacher.Where(s => s.MobilePhone.Contains(words)).ToList():new List<Teacher>();
                var emailList = searchFormat.Email?_ablemusicContext.Teacher.Where(s => s.Email.Contains(words)).ToList():new List<Teacher>();
                result.Data = firstNameList.Union(lastNameList).Union(genderList).Union(mobilePhoneList)
                    .Union(emailList);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            return Ok(result);
        }
        
        //DELETE http://localhost:5000/api/teacher/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                var item = _ablemusicContext.Teacher.FirstOrDefault(s => s.TeacherId == id);
                if (item == null)
                {
                    throw new Exception("teacher does not exist.");
                }
                item.IsActivate = 0;
                _ablemusicContext.Update(item);
                await _ablemusicContext.SaveChangesAsync();
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

        //GET: http://localhost:5000/api/teacher
        //this controller is for prepare for register custom one to one course
        [HttpGet]
        public async Task<ActionResult> GetTeacher()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                Random r = new Random();
                result.IsSuccess = true;
                var teachers = await _ablemusicContext.Teacher
                    .Include(s => s.TeacherLanguage)
                    .ThenInclude(s => s.Lang)
                    .Include(s => s.TeacherQualificatiion)
                    .ThenInclude(s => s.Quali)
                    .Include(s => s.TeacherCourse)
                    .ThenInclude(s => s.Course)
                    .Include(s => s.AvailableDays)
                    .ThenInclude(s=>s.Org)
                    .Include(s=>s.TeacherCourse)
                    .ThenInclude(s=>s.Course)
                    .Where(s => s.IsActivate == 1)
                    .Select(q => new
                    {
                        q.TeacherId,q.FirstName,q.LastName,q.Dob,Gender=Convert.ToBoolean(q.Gender)?"Male":"Female",
                        q.IrdNumber,q.IdType,IdPhoto=IsNull(q.IdPhoto)?null:String.Format("{0}?{1}",q.IdPhoto,r.Next()),q.IdNumber,q.HomePhone,q.MobilePhone,q.Email,
                        Photo=IsNull(q.Photo)?null:String.Format("{0}?{1}",q.Photo,r.Next()),q.ExpiryDate,
                        CV=IsNull(q.CvUrl)?null:String.Format("{0}?{1}",q.CvUrl,r.Next()),
                        OtherFile=IsNull(q.OtherfileUrl)?null:String.Format("{0}?{1}",q.OtherfileUrl,r.Next()),
                        Form=IsNull(q.FormUrl)?null:String.Format("{0}?{1}",q.FormUrl,r.Next()),
                        q.IsLeft,q.IsContract,q.InvoiceTemplate,q.Ability,q.Comment,q.Level,
                        q.AvailableDays,q.TeacherLanguage,q.TeacherQualificatiion,TeacherWageRate =q.TeacherWageRates.FirstOrDefault(s=>s.IsActivate==1)
                        
                    })
                    .ToListAsync();
                    
                result.Data = teachers;

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        //PUT: http://localhost:5000/api/teacher/:teacherid
        [HttpPut("{TeacherId}")]
        [CheckModelFilter]
        public async Task<IActionResult> TeacherUpdate([FromForm] string details,short TeacherId,
            [FromForm(Name = "IdPhoto")] IFormFile IdPhoto, [FromForm(Name = "Photo")] IFormFile Photo,
            [FromForm(Name="CV")] IFormFile CV,[FromForm(Name="Form")] IFormFile Form, [FromForm(Name = "Other")] IFormFile Other)
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<TeachersUpdate>(details);
                    if (await _ablemusicContext.Teacher.FirstOrDefaultAsync(s => s.TeacherId == TeacherId) == null)
                    {
                        return NotFound(DataNotFound(result));
                    }
                    var teacher = await _ablemusicContext.Teacher.FirstOrDefaultAsync(s => s.TeacherId == TeacherId);
                    _mapper.Map(detailsJson, teacher);

                    
                    //start update language
                    var teacherLanguages = _ablemusicContext.TeacherLanguage.Where(s => s.TeacherId == TeacherId);
                    teacherLanguages.ToList().ForEach(s => { _ablemusicContext.Remove(s); });
                    await _ablemusicContext.SaveChangesAsync();
                    detailsJson.Language.ForEach(s =>
                    {
                        _ablemusicContext.Add(new TeacherLanguage {TeacherId = teacher.TeacherId, LangId = s});
                    });
                    await _ablemusicContext.SaveChangesAsync();
                    //end
                    
                    //start update qualification
                    var teacherqualifications = _ablemusicContext.TeacherQualificatiion.Where(s => s.TeacherId == TeacherId);
                    teacherqualifications.ToList().ForEach(s => { _ablemusicContext.Remove(s);});
                    await _ablemusicContext.SaveChangesAsync();
                    detailsJson.Qualificatiion.ForEach(s=>
                    {
                        _ablemusicContext.Add(new TeacherQualificatiion{TeacherId = teacher.TeacherId, QualiId = s});
                    });
                    await _ablemusicContext.SaveChangesAsync();
                    //end
                    
                    //start update day of week
                    if (detailsJson.DayOfWeek.Count != 7)
                    {
                        throw new Exception("Day Of Week List must be length 7.");
                    }

                    var teacherDayOfWeek = _ablemusicContext.AvailableDays.Where(s => s.TeacherId == TeacherId);
                    teacherDayOfWeek.ToList().ForEach(s => { _ablemusicContext.Remove(s);});
                    await _ablemusicContext.SaveChangesAsync();
                    byte i = 1;
                    detailsJson.DayOfWeek.ForEach(s =>
                    {
                        if (s.Count != 0)
                        {
                            s.ForEach(w =>
                            {
                                _ablemusicContext.Add(new AvailableDays{TeacherId = teacher.TeacherId, DayOfWeek = i, CreatedAt = toNZTimezone(DateTime.UtcNow),OrgId = w});
                            });
                        }
                        i++;
                    });
                    await _ablemusicContext.SaveChangesAsync();
                    //end
                    
                    //start uploading the images
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff"); 
                    if (IdPhoto != null)
                    {
                        
                        teacher.IdPhoto = $"images/tutor/IdPhotos/{teacher.TeacherId+strDateTime+Path.GetExtension(IdPhoto.FileName)}";
                        _ablemusicContext.Update(teacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(IdPhoto,"tutor/IdPhotos/",teacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                        
                    }

                    if (Photo != null)
                    {
                        teacher.Photo = $"images/tutor/Photos/{teacher.TeacherId+strDateTime+Path.GetExtension(Photo.FileName)}";
                        _ablemusicContext.Update(teacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Photo,"tutor/Photos/", teacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (CV != null)
                    {
                        teacher.CvUrl = $"images/tutor/CV/{teacher.TeacherId+strDateTime+Path.GetExtension(CV.FileName)}";
                        _ablemusicContext.Update(teacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(CV,"tutor/CV/",teacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Form != null)
                    {
                        teacher.FormUrl = $"images/tutor/Form/{teacher.TeacherId+strDateTime+Path.GetExtension(Form.FileName)}";
                        _ablemusicContext.Update(teacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Form,"tutor/Form/",teacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Other != null)
                    {
                        teacher.OtherfileUrl =$"images/tutor/Other/{teacher.TeacherId+strDateTime+Path.GetExtension(Other.FileName)}";
                        _ablemusicContext.Update(teacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Other,"tutor/Other/",teacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }
                    
                    await _ablemusicContext.SaveChangesAsync();
                    //end uploading images
                    
                    dbContextTransaction.Commit();
                }

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            result.Data = "success";
            return Ok(result);
        }
        
        //POST: http://localhost:5000/api/teacher
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> TeacherRegister([FromForm] string details, [FromForm(Name = "IdPhoto")] IFormFile IdPhoto,[FromForm(Name ="Photo" )] IFormFile Photo,
            [FromForm(Name="CV")] IFormFile CV,[FromForm(Name="Form")] IFormFile Form, [FromForm(Name = "Other")] IFormFile Other)
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<TeachersRegister>(details);
                    if (await _ablemusicContext.Teacher.FirstOrDefaultAsync(s => s.IdNumber == detailsJson.IDNumber) !=
                        null)
                    {
                        throw new Exception("Teacher has exist.");
                    }
                    
                    var newTeacher = new Teacher();
                    _mapper.Map(detailsJson,newTeacher);
                    newTeacher.IsActivate = 1;
                    _ablemusicContext.Add(newTeacher);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    
                    var newUser = new User
                    {
                        UserName = newTeacher.Email,Password = "helloworld",
                        CreatedAt = DateTime.Now,RoleId = 1, IsActivate = 1
                        
                    };
                    _ablemusicContext.Add(newUser);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    newTeacher.UserId = newUser.UserId;
                    _ablemusicContext.Update(newTeacher);
                    await _ablemusicContext.SaveChangesAsync();

                    
                    detailsJson.Language.ForEach(s =>{ _ablemusicContext.Add(new TeacherLanguage {TeacherId = newTeacher.TeacherId, LangId = s});});
                    await _ablemusicContext.SaveChangesAsync();
                    
                    detailsJson.Qualificatiion.ForEach(s=>
                        {
                            newTeacherQualification = new TeacherQualificatiion
                                {TeacherId = newTeacher.TeacherId, QualiId = s};
                            _ablemusicContext.Add(newTeacherQualification);
                        });
                    await _ablemusicContext.SaveChangesAsync();

                    if (detailsJson.DayOfWeek.Count != 7)
                    {
                        throw new Exception("Day Of Week List must be length 7.");
                    }

                    byte i = 1;
                    detailsJson.DayOfWeek.ForEach(s =>
                    {
                        if (s.Count != 0)
                        {
                            s.ForEach(w =>
                                {
                                    DayList = new AvailableDays
                                    {
                                        TeacherId = newTeacher.TeacherId, DayOfWeek = i, CreatedAt = toNZTimezone(DateTime.UtcNow),
                                        OrgId = w
                                    };
                                    _ablemusicContext.Add(DayList);
                                });
                        }

                        i++;
                    });

                    await _ablemusicContext.SaveChangesAsync();
                    //file upload part
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff"); 
                    if (IdPhoto != null)
                    {
                        
                        newTeacher.IdPhoto = $"images/tutor/IdPhotos/{newTeacher.TeacherId+strDateTime+Path.GetExtension(IdPhoto.FileName)}";
                        _ablemusicContext.Update(newTeacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(IdPhoto,"tutor/IdPhotos/",newTeacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                        
                    }

                    if (Photo != null)
                    {
                        newTeacher.Photo = $"images/tutor/Photos/{newTeacher.TeacherId+strDateTime+Path.GetExtension(Photo.FileName)}";
                        _ablemusicContext.Update(newTeacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Photo,"tutor/Photos/", newTeacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (CV != null)
                    {
                        newTeacher.CvUrl = $"images/tutor/CV/{newTeacher.TeacherId+strDateTime+Path.GetExtension(CV.FileName)}";
                        _ablemusicContext.Update(newTeacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(CV,"tutor/CV/",newTeacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Form != null)
                    {
                        newTeacher.FormUrl = $"images/tutor/Form/{newTeacher.TeacherId+strDateTime+Path.GetExtension(Form.FileName)}";
                        _ablemusicContext.Update(newTeacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Form,"tutor/Form/",newTeacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (Other != null)
                    {
                        newTeacher.OtherfileUrl =$"images/tutor/Other/{newTeacher.TeacherId+strDateTime+Path.GetExtension(Other.FileName)}";
                        _ablemusicContext.Update(newTeacher);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(Other,"tutor/Other/",newTeacher.TeacherId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    
                    dbContextTransaction.Commit();
                    result.Data = "Success!";
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