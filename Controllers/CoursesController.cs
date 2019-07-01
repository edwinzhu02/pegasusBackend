using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : BasicController
    {
        private readonly IMapper _mapper;

        public CoursesController(ablemusicContext ablemusicContext, IMapper mapper, ILogger<CoursesController> log) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            Result<List<Object>> result = new Result<List<Object>>();
            result.Data = new List<Object>();
            List<Course> courses = new List<Course>();
            List<Lookup> lookups = new List<Lookup>();
            try
            {
                courses = await (from c in _ablemusicContext.Course
                                  join cc in _ablemusicContext.CourseCategory on c.CourseCategoryId equals cc.CourseCategoryId
                                  select new Course
                                  {
                                      CourseId = c.CourseId,
                                      CourseName = c.CourseName,
                                      CourseType = c.CourseType,
                                      Level = c.Level,
                                      Duration = c.Duration,
                                      Price = c.Price,
                                      CourseCategoryId = c.CourseCategoryId,
                                      TeacherLevel = c.TeacherLevel,
                                      CourseCategory = new CourseCategory
                                      {
                                          CourseCategoryId = cc.CourseCategoryId,
                                          CourseCategoryName = cc.CourseCategoryName,
                                          Course = null
                                      }
                                  }).ToListAsync();
                lookups = await (from l in _ablemusicContext.Lookup
                                where l.LookupType == 1 || l.LookupType == 4 || l.LookupType == 6 || l.LookupType == 8
                                select new Lookup
                                {
                                    PropName = l.PropName,
                                    PropValue = l.PropValue,
                                    LookupType = l.LookupType
                                }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            foreach (var course in courses)
            {
                result.Data.Add(new
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseType = course.CourseType,
                    CourseTypeName = lookups.Where(l => l.PropValue == course.CourseType && l.LookupType == 6).FirstOrDefault().PropName,
                    Level = course.Level,
                    LevelName = lookups.Where(l => l.PropValue == course.Level && l.LookupType == 4).FirstOrDefault().PropName,
                    Duration = course.Duration,
                    DurationName = lookups.Where(l => l.PropValue == course.Duration && l.LookupType == 8).FirstOrDefault().PropName,
                    Price = course.Price,
                    CourseCategoryId = course.CourseCategoryId,
                    TeacherLevel = course.TeacherLevel,
                    TeacherLevelName = lookups.Where(l => l.PropValue == course.TeacherLevel && l.LookupType == 1).FirstOrDefault().PropName,
                    CourseCategory = course.CourseCategory,
                });
            }

            return Ok(result);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetCoursesCate()
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.CourseCategory.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        // PUT: api/Courses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, [FromBody] CourseViewModel courseViewModel)
        {
            var result = new Result<string>();
            Type courseType = typeof(Course);
            Course course = new Course();
            _mapper.Map(courseViewModel, course);
            Course updateCourse = new Course();
            try
            {
                updateCourse = await _ablemusicContext.Course.Where(c => c.CourseId == id).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }

            if (updateCourse == null)
            {
                return NotFound(result);
            }
            UpdateTable(course, courseType, updateCourse);
            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            return Ok(result);

        }

        // POST: api/Courses
        [HttpPost]
        [CheckModelFilter]
        public async Task<ActionResult<Course>> PostCourse(CourseViewModel courseViewModel)
        {
            Result<List<Course>> result = new Result<List<Course>>();
            Course course = new Course();
            _mapper.Map(courseViewModel, course);
            Course courseExists = new Course();
            try
            {
                courseExists = await _ablemusicContext.Course.Where(c => c.CourseId == course.CourseId).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }

            if (courseExists != null)
            {
                result.ErrorMessage = "The input id is already exists";
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            try
            {
                await _ablemusicContext.Course.AddAsync(course);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                _ablemusicContext.Remove(course);
                return BadRequest(result);
            }

            return Ok(result);
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            Result<List<Course>> result = new Result<List<Course>>();

            try
            {
                var course = await _ablemusicContext.Course.FindAsync(id);
                if (course == null)
                {
                    result.ErrorMessage = "id not found";
                    result.IsSuccess = false;
                    result.IsFound = false;
                    return NotFound(result);
                }

                _ablemusicContext.Course.Remove(course);
                await _ablemusicContext.SaveChangesAsync();
                result.IsFound = true;
                result.IsSuccess = true;
                return Ok(result);
            }
            catch (Exception e)
            {
                if(e.HResult == -2146233088)
                {
                    result.ErrorMessage = "The selected course is not allowed to delete";
                    result.IsSuccess = false;
                    return BadRequest(result);
                }
                result.ErrorMessage = e.ToString();
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }        
        }

        private bool CourseExists(int id)
        {
            return _ablemusicContext.Course.Any(e => e.CourseId == id);
        }
    }
}