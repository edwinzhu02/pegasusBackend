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
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
namespace Pegasus_backend.Controllers.Register
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherRegisterController: BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;
        private IFormFile file;
        private IFormFile teacherIdPhoto;
        private IFormFile teacherPhoto;
        private TeacherQualificatiion newTeacherQualification;
        private TeacherLanguage newTeacherLanguage;
        private AvailableDays DayList;
        public TeacherRegisterController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        //POST: http://localhost:5000/api/teacherregister
        [HttpPost]
        [CheckModelFilter]
        public ActionResult<Result<string>> TeacherRegister([FromForm] TeachersRegister details)
        {
            Result<string> result = new Result<string>();
            using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
            {
                try
                {
                    if (_pegasusContext.Teacher.FirstOrDefault(s=>s.IdNumber == details.IDNumber) != null)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Teacher has exist.";
                        return BadRequest(result);
                    }
                    
                    var newTeacher = new Teacher();
                    _mapper.Map(details,newTeacher);
                    _pegasusContext.Add(newTeacher);
                    _pegasusContext.SaveChanges();
                    
                    //process teacher id photo upload and id photo upload
                    //here
                    var requestForm = Request.Form;
                    if (requestForm.Files.Count == 0)
                    {
                        result.ErrorMessage = "Id photo required";
                        result.IsSuccess = false;
                        return BadRequest(result);

                    }
                    
                    if (requestForm.Files.Count == 1)
                    {
                        if (requestForm.Files[0].Name == "IdPhoto")
                        {
                            file = requestForm.Files[0];
                            try
                            {
                                //call basic controller method
                                UploadFile(file,requestForm.Files[0].Name);
                                //add to db
                                newTeacher.IdPhoto = $"images/TeacherIdPhotos/{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
                                _pegasusContext.Update(newTeacher);
                                _pegasusContext.SaveChanges();

                            }
                            catch (Exception ex)
                            {
                                result.ErrorMessage = ex.Message;
                                result.IsSuccess = false;
                                return BadRequest(result);
                            }
                            
                        }
                        else
                        {
                            result.ErrorMessage = "The id Photo must be uploaded firstly";
                            result.IsSuccess = false;
                            return BadRequest(result);
                        }
                    }else if (requestForm.Files.Count == 2)
                    {
                        if (requestForm.Files[0].Name == "IdPhoto" && requestForm.Files[1].Name == "Photo")
                        {
                            teacherIdPhoto = requestForm.Files[0];
                            teacherPhoto = requestForm.Files[1];
                        }else if (requestForm.Files[1].Name == "IdPhoto" && requestForm.Files[0].Name == "Photo")
                        {
                            teacherIdPhoto = requestForm.Files[1];
                            teacherPhoto = requestForm.Files[0];
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.ErrorMessage = "These two post key name of images must be IdPhoto and Photo";
                        }

                        try
                        {

                            //call upload file in basic controller
                            UploadFile(teacherPhoto,"Photo");
                            UploadFile(teacherIdPhoto,"IdPhoto");

                            newTeacher.IdPhoto = $"images/TeacherIdPhotos/{ContentDispositionHeaderValue.Parse(teacherIdPhoto.ContentDisposition).FileName.Trim('"')}";
                            newTeacher.Photo = $"images/TeacherImages/{ContentDispositionHeaderValue.Parse(teacherPhoto.ContentDisposition).FileName.Trim('"')}";

                            _pegasusContext.Update(newTeacher);
                            _pegasusContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            result.IsSuccess = false;
                            result.ErrorMessage = ex.Message;
                            return BadRequest(result);
                        }

                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "No more than 2 images.";
                        return BadRequest(result);
                    }



                    foreach (var quali in details.Qualification)
                    {
                        newTeacherQualification = new TeacherQualificatiion()
                        {
                            TeacherId = newTeacher.TeacherId,
                            QualiId = _pegasusContext.Qualification.FirstOrDefault(s => s.QualiName == quali)
                                .QualiId
                        };
                        _pegasusContext.Add(newTeacherQualification);
                    }

                    _pegasusContext.SaveChanges();
                    
                    
                    foreach (var lan in details.Language)
                    {
                        newTeacherLanguage = new TeacherLanguage()
                        {
                            TeacherId = newTeacher.TeacherId,
                            LangId = _pegasusContext.Language.FirstOrDefault(s=>s.LangName == lan).LangId
                        };
                        _pegasusContext.Add(newTeacherLanguage);
                    }
                    
                    _pegasusContext.SaveChanges();

                    if (details.DayOfWeek.Count != 7)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Day Of Week List must be length 7.";
                        return BadRequest(result);
                    }
                    
                    byte i = 1;
                    details.DayOfWeek.ForEach(s =>
                    {
                        
                        if (s.Count != 0)
                        {
                            s.ForEach(w =>
                            {
                                DayList = new AvailableDays()
                                {
                                    TeacherId = newTeacher.TeacherId,
                                    DayOfWeek = i,
                                    CreatedAt = DateTime.Now,
                                    OrgId = _pegasusContext.Org.FirstOrDefault(a=>a.OrgName == w).OrgId
                                    
                                };
                                _pegasusContext.Add(DayList);
                            });
                        }

                        i++;
                    });

                    _pegasusContext.SaveChanges();

                    
                    dbContextTransaction.Commit();

                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
            }

            result.Data = "teacher added successfully";
            return Ok(result);
        }
    }
}