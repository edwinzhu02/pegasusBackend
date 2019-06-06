using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherCourseController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private readonly IMapper _mapper;

        public TeacherCourseController(pegasusContext.ablemusicContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        //GET api/teachercourse
        [HttpGet]
        public async Task<IActionResult> GetTeacherCourse()
        {
            Result<Object> result = new Result<Object>();
            try
            {
                result.Data = await _pegasusContext.TeacherCourse
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Course)
                    .ThenInclude(s=>s.GroupCourseInstance)
                    .Include(s=>s.Course)
                    .ThenInclude(s=>s.CourseCategory)                    
                    .Select(s=> new
                    {
                        s.TeacherCourseId,s.CourseId,s.TeacherId,s.HourlyWage,
                        Course=new
                        {
                            s.Course.CourseId,s.Course.CourseName,s.Course.Level,s.Course.Duration,
                            s.Course.Price, s.Course.CourseType
                        },
                        Teacher= new {s.Teacher.TeacherId,s.Teacher.FirstName,s.Teacher.LastName,s.Teacher.Level}
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);

        }
        
        //PUT api/teachercourse
        [HttpPut]
        [CheckModelFilter]
        public async Task<IActionResult> UpdateTeacherCourse([FromBody] TeacherCourseRegister model)
        {
            Decimal? houlyWage = 0;
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {

                    if (_pegasusContext.Teacher.FirstOrDefault(s => s.TeacherId == model.TeacherId) == null)
                    {
                        throw new Exception("Teacher id does not found");
                    }
                    
                    var teacherCourses = _pegasusContext.TeacherCourse.Where(s => s.TeacherId == model.TeacherId);
                    teacherCourses.ToList().ForEach(s => { _pegasusContext.Remove(s); });
                    await _pegasusContext.SaveChangesAsync();

                    
                    //delete older teacher wage rate
                    var oldteacherWageRate =
                        _pegasusContext.TeacherWageRates.FirstOrDefault(s => s.TeacherId == model.TeacherId);
                    oldteacherWageRate.IsActivate = 0;
                    _pegasusContext.Update(oldteacherWageRate);
                    await _pegasusContext.SaveChangesAsync();
                    
                    var teacherWageRate = new TeacherWageRates
                    {
                        TeacherId = model.TeacherId, PianoRates = model.TeacherWageRates.PianoRates, TheoryRates = model.TeacherWageRates.TheoryRates,
                        GroupRates = model.TeacherWageRates.GroupRates, OthersRates = model.TeacherWageRates.OthersRates,
                        CreateAt = DateTime.Now, IsActivate = 1
                    };
                    _pegasusContext.Add(teacherWageRate);
                    await _pegasusContext.SaveChangesAsync();
                    
                    model.Courses.ForEach(s =>
                    {
                        var course = _pegasusContext.Course.Include(q=>q.CourseCategory).FirstOrDefault(w => w.CourseId == s);
                        if (course == null)
                        {
                            throw new Exception("Course of course id: " + s + " does not exist.");
                        }

                        if (course.CourseType == 2)
                        {
                            houlyWage = teacherWageRate.GroupRates;
                            
                        }
                        else
                        {
                            if (course.CourseCategory.CourseCategoryId == 1)
                            {
                                houlyWage = teacherWageRate.PianoRates;
                            }

                            else if (course.CourseCategory.CourseCategoryId == 7)
                            {
                                houlyWage = teacherWageRate.TheoryRates;
                            }
                            else
                            {
                                houlyWage = teacherWageRate.OthersRates;
                            }
                        }
                        var teacherCourse = new TeacherCourse
                        {
                            CourseId = s, TeacherId = model.TeacherId,HourlyWage = houlyWage,
                            
                        };
                        _pegasusContext.Add(teacherCourse);
                    });
                    
                    await _pegasusContext.SaveChangesAsync();
                    dbContextTransaction.Commit();
                }

                result.Data = "Teacher courses are updated successfully";

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        //POST api/teachercourse
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> AddTeacherCourse([FromBody] TeacherCourseRegister model)
        {
            Decimal? houlyWage = 0;
            Result<String> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    if (_pegasusContext.TeacherCourse.Where(s => s.TeacherId == model.TeacherId).ToList().Count != 0)
                    {
                        throw new Exception("Teacher course has already added for this teacher");
                    }

                    if (_pegasusContext.Teacher.FirstOrDefault(s => s.TeacherId == model.TeacherId) == null)
                    {
                        throw new Exception("Teacher id does not found");
                    }
                    
                    var teacherWageRate = new TeacherWageRates
                    {
                        TeacherId = model.TeacherId, PianoRates = model.TeacherWageRates.PianoRates, TheoryRates = model.TeacherWageRates.TheoryRates,
                        GroupRates = model.TeacherWageRates.GroupRates, OthersRates = model.TeacherWageRates.OthersRates,
                        CreateAt = DateTime.Now, IsActivate = 1
                    };
                    _pegasusContext.Add(teacherWageRate);
                    await _pegasusContext.SaveChangesAsync();
                    
                    model.Courses.ForEach(s =>
                    {
                        var course = _pegasusContext.Course.Include(q=>q.CourseCategory).FirstOrDefault(w => w.CourseId == s);
                        if (course == null)
                        {
                            throw new Exception("Course of course id: " + s + " does not exist.");
                        }

                        if (course.CourseType == 2)
                        {
                            houlyWage = teacherWageRate.GroupRates;
                            
                        }
                        else
                        {
                            if (course.CourseCategory.CourseCategoryId == 1)
                            {
                                houlyWage = teacherWageRate.PianoRates;
                            }

                            else if (course.CourseCategory.CourseCategoryId == 7)
                            {
                                houlyWage = teacherWageRate.TheoryRates;
                            }
                            else
                            {
                                houlyWage = teacherWageRate.OthersRates;
                            }
                        }
                        var teacherCourse = new TeacherCourse
                        {
                            CourseId = s, TeacherId = model.TeacherId,HourlyWage = houlyWage,
                            
                        };
                        _pegasusContext.Add(teacherCourse);
                    });
                    await _pegasusContext.SaveChangesAsync();
                    
                    dbContextTransaction.Commit();
                    
                }
                
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = "Teacher courses created successfully";
            return Ok(result);
        }
    }
}