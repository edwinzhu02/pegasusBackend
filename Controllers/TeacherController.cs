using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : BasicController
    {

        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;
        private TeacherLanguage newTeacherLanguage;
        private TeacherQualificatiion newTeacherQualification;
        private AvailableDays DayList;

        public TeacherController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        
        //DELETE http://localhost:5000/api/teacher
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                var item = _pegasusContext.Teacher.FirstOrDefault(s => s.TeacherId == id);
                item.IsActivate = 0;
                _pegasusContext.Update(item);
                await _pegasusContext.SaveChangesAsync();
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
                result.IsSuccess = true;
                var teachers = await _pegasusContext.Teacher
                    .Include(s => s.TeacherLanguage)
                    .ThenInclude(s => s.Lang)
                    .Include(s => s.TeacherQualificatiion)
                    .ThenInclude(s => s.Quali)
                    .Include(s => s.TeacherCourse)
                    .ThenInclude(s => s.Course)
                    .Include(s => s.AvailableDays)
                    .ThenInclude(s=>s.Org)
                    .Where(s => s.IsActivate == 1)
                    .Select(q => new
                    {
                        q.TeacherId,q.FirstName,q.LastName,q.Dob,q.Gender,q.IrdNumber,q.IdType,q.IdPhoto,q.IdNumber,q.HomePhone,q.MobilePhone,q.Email,q.Photo,q.ExpiryDate,
                        /*AvailableDays = q.AvailableDays.Select(w=> new {w.DayOfWeek,w.OrgId, w.Org.OrgName}),
                        TeacherLanguage = q.TeacherLanguage.Select(w=>new {w.LangId,w.Lang.LangName}),
                        TeacherQualificatiion = q.TeacherQualificatiion.Select(w=> new {w.QualiId,w.Quali.QualiName}),
                        TeacherCourse = q.TeacherCourse.Select(w=> new {w.CourseId,w.TeacherCourseId,w.Course.CourseName,w.Course.Level})*/
                        q.AvailableDays,q.TeacherLanguage,q.TeacherQualificatiion,q.TeacherCourse
                        
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
        
        //PUT: http://localhost:5000/api/teacher
        /*[HttpPut]
        [CheckModelFilter]
        public async Task<IActionResult> TeacherUpdate([FromForm] string details,
            [FromForm(Name = "IdPhoto")] IFormFile IdPhoto, [FromForm(Name = "Photo")] IFormFile Photo)
        {
            
        }*/
        
        //POST: http://localhost:5000/api/teacher
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> TeacherRegister([FromForm] string details, [FromForm(Name = "IdPhoto")] IFormFile IdPhoto,[FromForm(Name ="Photo" )] IFormFile Photo )
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
                    newTeacher.IsActivate = 1;
                    _pegasusContext.Add(newTeacher);
                    await _pegasusContext.SaveChangesAsync();

                    if (IdPhoto != null)
                    {
                        newTeacher.IdPhoto = $"images/TeacherIdPhotos/{newTeacher.TeacherId}";
                        _pegasusContext.Update(newTeacher);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(IdPhoto,"IdPhoto",newTeacher.TeacherId);
                    }

                    if (Photo != null)
                    {
                        newTeacher.Photo = $"images/TeacherImages/{newTeacher.TeacherId}";
                        _pegasusContext.Update(newTeacher);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(Photo,"Photo", newTeacher.TeacherId);
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