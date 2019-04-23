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

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController: BasicController

    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private TeacherQualificatiion newTeacherQualification;
        private TeacherLanguage newTeacherLanguage;
        private AvailableDays DayList;
        private Learner newLearner;
        
        //image for learner
        private IFormFile image;
        //image for ABRSM
        private IFormFile ABRSM;
        
        //only one image case
        private IFormFile file;
        private LearnerGroupCourse newLearnerGroupCourse;
        private IFormFile teacherIdPhoto;
        private IFormFile teacherPhoto;
        private readonly IMapper _mapper;
        
        
        public RegisterController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }

        
        //GET: http://localhost:5000/api/register/teacher
        [HttpGet]
        [Route("teacher")]
        public ActionResult<Result<DetailsForTeacherRegister>> GetDetailsForTeacher()
        {
            Result<DetailsForTeacherRegister> result = new Result<DetailsForTeacherRegister>();
            try
            {
                DetailsForTeacherRegister details = new DetailsForTeacherRegister()
                {
                    qualifications = _pegasusContext.Qualification.ToList(),
                    Languages = _pegasusContext.Language.ToList(),
                    Orgs = _pegasusContext.Org.ToList()
                };
                result.Data = details;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            return Ok(result);
        }




        //POST: http://api/localhost:5000/register/teacher
        [HttpPost]
        [Route("teacher")]
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
                    _mapper.Map(details,newTeacher );
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
                            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            var folderName = Path.Combine("wwwroot", "images", "TeacherIdPhotos");
                            try
                            {
                                //call basic controller method
                                UploadFile(fileName,file,folderName);
                                //add to db
                                newTeacher.IdPhoto = $"images/TeacherIdPhotos/{fileName}";
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
                            var photoID = ContentDispositionHeaderValue.Parse(teacherIdPhoto.ContentDisposition).FileName.Trim('"');
                        
                            var photo = ContentDispositionHeaderValue.Parse(teacherPhoto.ContentDisposition).FileName.Trim('"');
                            
                            var photoIdFolderName = Path.Combine("wwwroot", "images", "TeacherIdPhotos");
                            var photoFolderName = Path.Combine("wwwroot", "images", "TeacherImages");
                            
                            //call upload file in basic controller
                            UploadFile(photoID,teacherIdPhoto,photoIdFolderName);
                            UploadFile(photo,teacherPhoto,photoFolderName);

                            newTeacher.IdPhoto = $"images/TeacherIdPhotos/{photoID}";
                            newTeacher.Photo = $"images/TeacherImages/{photo}";

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
                    result.ErrorMessage = ex.ToString();
                    return BadRequest(result);
                }
            }

            result.Data = "teacher added successfully";
            return Ok(result);
        }
        
        
        //POST: http://localhost:5000/api/register/student
        [HttpPost]
        [Route("student")]
        [CheckModelFilter]
        public IActionResult StudentRegister([FromForm] StudentRegister details)
        {
            Result<string> result = new Result<string>();
            
            //Handle upload Data
            using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
            {
                try
                {
                    newLearner = new Learner();
                    _mapper.Map(details, newLearner);

                    _pegasusContext.Add(newLearner);
                    _pegasusContext.SaveChanges();
                    Parent newParent = new Parent();
                    _mapper.Map(details.Parents, newParent);
                    newParent.CreatedAt = DateTime.Now;
                    newParent.LearnerId = newLearner.LearnerId;
                    _pegasusContext.Add(newParent);
                    _pegasusContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }

                try
                {
                    details.GroupCourses.ForEach(s =>
                    {    
                            newLearnerGroupCourse = new LearnerGroupCourse()
                            {
                                LearnerId = newLearner.LearnerId,
                                GroupCourseInstanceId = s,
                                CreatedAt = DateTime.Now
                            };
                        

                        _pegasusContext.Add(newLearnerGroupCourse);
                    });

                    _pegasusContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
                


                //Handle uploading image
                var requestForm = Request.Form;
                //Case1 : no image upload when student account created
                if (requestForm.Files.Count == 0)
                {
                    newLearner.IsAbrsmG5 = 0;
                    //Case2: only one image upload(this may be learner image or ABRSM level5 certificate)
                }
                else if (requestForm.Files.Count == 1)
                {
                    if (requestForm.Files[0].Name == "image")
                    {
                        file = requestForm.Files[0];
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var folderName = Path.Combine("wwwroot", "images", "LearnerImages");
                        try
                        {
                            
                             UploadFile(fileName,file,folderName);

                            newLearner.Photo = $"images/LearnerImages/{fileName}";
                            newLearner.IsAbrsmG5 = 0;
                            _pegasusContext.Update(newLearner);
                            _pegasusContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = ex.Message;
                            return BadRequest(result);
                        }
                    }
                    else if (requestForm.Files[0].Name == "ABRSM")
                    {
                        file = requestForm.Files[0];
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var folderName = Path.Combine("wwwroot", "images", "ABRSM_Grade5_Certificate");
                        try
                        {
                            
                            UploadFile(fileName,file,folderName);
                            
                            newLearner.G5Certification = $"images/ABRSM_Grade5_Certificate/{fileName}";
                            newLearner.IsAbrsmG5 = 1;
                            _pegasusContext.Update(newLearner);
                            _pegasusContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = ex.Message;
                            return BadRequest(result);
                        }

                    }
                    else
                    {
                        result.ErrorMessage = "The post key name of images must be image or ABRSM";
                        return BadRequest(result);
                    }

                    //Case 3: both learner image and ABRSM image upload when student account created
                }
                else if (requestForm.Files.Count == 2)
                {
                    if (requestForm.Files[0].Name == "image" && requestForm.Files[1].Name == "ABRSM")
                    {
                        image = requestForm.Files[0];
                        ABRSM = requestForm.Files[1];
                    }
                    else if (requestForm.Files[1].Name == "image" && requestForm.Files[0].Name == "ABRSM")
                    {
                        image = requestForm.Files[1];
                        ABRSM = requestForm.Files[0];
                    }
                    else
                    {
                        result.ErrorMessage = "These two post key name of images must be image and ABRSM";
                        return BadRequest(result);
                    }


                    var imageFileName = ContentDispositionHeaderValue.Parse(image.ContentDisposition).FileName
                        .Trim('"');
                    var ABRSM_FileName = ContentDispositionHeaderValue.Parse(ABRSM.ContentDisposition).FileName
                        .Trim('"');
                    //ABRSM image folder location
                    var ABRSM_FolderName = Path.Combine("wwwroot", "images", "ABRSM_Grade5_Certificate");
                    //Learner image folder location
                    var LearnerImagesFolderName = Path.Combine("wwwroot", "images", "LearnerImages");
                    try
                    {
                        UploadFile(imageFileName,image,LearnerImagesFolderName);
                        UploadFile(ABRSM_FileName,ABRSM,ABRSM_FolderName);
                        
                        newLearner.G5Certification = $"images/ABRSM_Grade5_Certificate/{ABRSM_FileName}";
                        newLearner.Photo = $"images/LearnerImages/{imageFileName}";
                        newLearner.IsAbrsmG5 = 1;
                        _pegasusContext.Update(newLearner);
                        _pegasusContext.SaveChanges();
                        
                    }
                    catch (Exception ex)
                    {
                        result.ErrorMessage = ex.Message;
                        return BadRequest(result);
                    }
                }
                //Case 4: no more than 2 image upload
                else
                {
                    result.ErrorMessage = "No more than 2 images.";
                    return BadRequest(result);
                }
                
                dbContextTransaction.Commit();
            }

            result.Data = "Student successfully added";
            return Ok(result);
        }
    }
}