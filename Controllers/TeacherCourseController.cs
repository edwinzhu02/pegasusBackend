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
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var teacherCourses = _pegasusContext.TeacherCourse.Where(s => s.TeacherId == model.TeacherId);
                    teacherCourses.ToList().ForEach(s => { _pegasusContext.Remove(s); });
                    await _pegasusContext.SaveChangesAsync();
                    
                    model.TeacherCourses.ForEach(s =>
                    {
                        var teacherCourse = new TeacherCourse();
                        _mapper.Map(s, teacherCourse);
                        teacherCourse.TeacherId = model.TeacherId;
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
            Result<String> result = new Result<string>();
            try
            {
                model.TeacherCourses.ForEach(s =>
                {
                    var teacherCourse = new TeacherCourse();
                    _mapper.Map(s, teacherCourse);
                    teacherCourse.TeacherId = model.TeacherId;
                    _pegasusContext.Add(teacherCourse);
                });
                await _pegasusContext.SaveChangesAsync();
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