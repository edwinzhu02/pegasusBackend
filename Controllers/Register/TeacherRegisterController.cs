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

namespace Pegasus_backend.Controllers.Register
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherRegisterController : BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;
        private TeacherLanguage newTeacherLanguage;
        private TeacherQualificatiion newTeacherQualification;
        private AvailableDays DayList;
        public TeacherRegisterController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
            
        }

        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> TeacherRegister([FromForm] string details, [FromForm] IList<IFormFile> IdPhoto,[FromForm] IList<IFormFile> Photo )
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<TeachersRegister>(details);
                    if (await _pegasusContext.Teacher.FirstOrDefaultAsync(s => s.IdNumber == detailsJson.IDNumber) !=
                        null)
                    {
                        throw new Exception("Teacher has exist.");
                    }
                    
                    var newTeacher = new Teacher();
                    _mapper.Map(detailsJson,newTeacher);
                    _pegasusContext.Add(newTeacher);
                    await _pegasusContext.SaveChangesAsync();

                    if (IdPhoto.Count == 0)
                    {
                        throw new Exception("Id photo required");
                    }
                    else
                    {
                        newTeacher.IdPhoto = $"images/TeacherIdPhotos/{ContentDispositionHeaderValue.Parse(IdPhoto[0].ContentDisposition).FileName.Trim('"')}";
                        _pegasusContext.Update(newTeacher);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(IdPhoto[0],"IdPhoto");
                    }

                    if (Photo.Count != 0)
                    {
                        newTeacher.Photo = $"images/TeacherImages/{ContentDispositionHeaderValue.Parse(Photo[0].ContentDisposition).FileName.Trim('"')}";
                        _pegasusContext.Update(newTeacher);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(Photo[0],"Photo");
                    }
                    
                    detailsJson.Language.ForEach(s =>
                        {
                            newTeacherLanguage = new TeacherLanguage {TeacherId = newTeacher.TeacherId, LangId = s};
                            _pegasusContext.Add(newTeacherLanguage);
                        });
                    await _pegasusContext.SaveChangesAsync();
                    
                    detailsJson.Qualificatiion.ForEach(s=>
                        {
                            newTeacherQualification = new TeacherQualificatiion
                                {TeacherId = newTeacher.TeacherId, QualiId = s};
                            _pegasusContext.Add(newTeacherQualification);
                        });
                    await _pegasusContext.SaveChangesAsync();

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
                                        TeacherId = newTeacher.TeacherId, DayOfWeek = i, CreatedAt = DateTime.Now,
                                        OrgId = w
                                    };
                                    _pegasusContext.Add(DayList);
                                });
                        }

                        i++;
                    });

                    await _pegasusContext.SaveChangesAsync();
                    
                    dbContextTransaction.Commit();
                    result.Data = "Sucess!";
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