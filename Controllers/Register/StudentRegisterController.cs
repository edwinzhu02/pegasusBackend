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
    public class StudentRegisterController: BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;
        private IFormFile file;
        private LearnerGroupCourse newLearnerGroupCourse;
        //image for learner
        private IFormFile image;
        //image for ABRSM
        private IFormFile ABRSM;
        private Learner newLearner;
        public StudentRegisterController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        //POST: http://localhost:5000/api/studentregister
        [HttpPost]
        [CheckModelFilter]
        public IActionResult StudentRegister([FromForm] StudentRegister details)
        {
            Result<string> result = new Result<string>();
            
            //Handle upload Data
            using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
            {
                try
                {
                    //Student
                    newLearner = new Learner();
                    _mapper.Map(details, newLearner);
                    newLearner.CreatedAt = DateTime.Now;
                    _pegasusContext.Add(newLearner);
                    _pegasusContext.SaveChanges();
                    
                    //Parent related to Student
                    pegasusContext.Parent newParent = new pegasusContext.Parent();
                    _mapper.Map(details.Parent, newParent);
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

                if (details.GroupCourses.Count != 0)
                {
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
                        try
                        {
                            UploadFile(file,"image");
                            newLearner.Photo = $"images/LearnerImages/{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
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
                        try
                        {
                            UploadFile(file,"ABRSM");
                            newLearner.G5Certification = $"images/ABRSM_Grade5_Certificate/{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
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
                    try
                    {
                        UploadFile(image,"image");
                        UploadFile(ABRSM,"ABRSM");
                        
                        newLearner.G5Certification = $"images/ABRSM_Grade5_Certificate/{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
                        newLearner.Photo = $"images/LearnerImages/{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
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