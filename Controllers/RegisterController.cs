using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController: ControllerBase

    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private TeacherQualificatiion newTeacherQualification;
        private TeacherLanguage newTeacherLanguage;
        private AvailableDays DayList;
        
        public RegisterController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        //GET http://api/localhost:5000/register/getQualification
        [HttpGet]
        [Route("getQualification")]
        public ActionResult<List<Qualification>> GetQualification()
        {
            return _pegasusContext.Qualification.ToList();
        }

        
        //GET http://api/localhost:5000/register/getLanguage
        [HttpGet]
        [Route("getLanguage")]
        public ActionResult<List<Language>> GetLanguage()
        {
            return _pegasusContext.Language.ToList();
        }
        
        //GET http://api/localhost:5000/register/getOrg
        [HttpGet]
        [Route("getOrg")]
        public ActionResult<List<pegasusContext.Org>> GetOrg()
        {
            return _pegasusContext.Org.ToList();
        }
        
        



        //POST: http://api/localhost:5000/register/teacher
        [HttpPost]
        [Route("teacher")]
        public ActionResult<Result<string>> TeacherRegister([FromBody] TeachersRegister details)
        {
            Result<string> result = new Result<string>();
            using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
            {
                try
                {
                    if (_pegasusContext.Teacher.FirstOrDefault(s=>s.IdNumber == details.IDNumber) != null)
                    {
                        result.ErrorCode = "401";
                        result.IsSuccess = false;
                        result.ErrorMessage = "Teacher has exist.";
                        return result;
                    }
                    var newTeacher = new Teacher()
                    {
                        FirstName = details.FirstName,
                        LastName =  details.LastName,
                        Dob = details.Dob,
                        Gender = details.Gender,
                        IrdNumber = details.IRDNumber,
                        IdType = details.IDType,
                        IdNumber = details.IDNumber,
                        HomePhone = details.HomePhone,
                        MobilePhone = details.PhoneNumber,
                        Email = details.Email,
                        ExpiryDate = details.IDExpireDate,
                    };
                    _pegasusContext.Add(newTeacher);
                    _pegasusContext.SaveChanges();

                    
                    foreach (var quali in details.Qualification)
                    {
                        newTeacherQualification = new TeacherQualificatiion()
                        {
                            TeacherId = _pegasusContext.Teacher.FirstOrDefault(s=>s.IdNumber == details.IDNumber).TeacherId,
                            QualiId = _pegasusContext.Qualification.FirstOrDefault(s=>s.QualiName==quali).QualiId
                        };
                        _pegasusContext.Add(newTeacherQualification);
                    }

                    _pegasusContext.SaveChanges();

                    foreach (var lan in details.Language)
                    {
                        newTeacherLanguage = new TeacherLanguage()
                        {
                            TeacherId = _pegasusContext.Teacher.FirstOrDefault(s=>s.IdNumber == details.IDNumber).TeacherId,
                            LangId = _pegasusContext.Language.FirstOrDefault(s=>s.LangName == lan).LangId
                        };
                        _pegasusContext.Add(newTeacherLanguage);
                    }
                    _pegasusContext.SaveChanges();

                    if (details.DayOfWeek.Count != 7)
                    {
                        result.ErrorCode = "401";
                        result.IsSuccess = false;
                        result.ErrorMessage = "Day Of Week List must be length 7.";
                        return result;
                    }
                    
                    details.DayOfWeek.ForEach(s =>
                    {
                        byte i = 1;
                        if (s.Count != 0)
                        {
                            s.ForEach(w =>
                            {
                                DayList = new AvailableDays()
                                {
                                    TeacherId = _pegasusContext.Teacher.FirstOrDefault(a=>a.IdNumber == details.IDNumber).TeacherId,
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
                    result.ErrorCode = "401";
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.ToString();
                }
            }

            return result;
        }
        
        
        //GET: http://localhost:5000/api/register/student
        [HttpPost]
        [Route("student")]
        public IActionResult StudentRegister([FromForm] StudentRegister details)
        {
            var requestForm = Request.Form;
            Result<string> result = new Result<string>();
            return Ok(result);
        }
    }
}