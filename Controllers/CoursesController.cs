using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;
        private readonly IMapper _mapper;

        public CoursesController(pegasusContext.ablemusicContext pegasusContext, IMapper mapper)
        {
            _ablemusicContext = pegasusContext;
            _mapper = mapper;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            Result<List<Course>> result = new Result<List<Course>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.Course.Include(c => c.CourseCategory).ToListAsync();
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
            var updateCourse = await _ablemusicContext.Course.Where(c => c.CourseId == id).FirstOrDefaultAsync();
            if (updateCourse == null)
            {
                return NotFound(DataNotFound(result));
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

            var courseExists = await _ablemusicContext.Course.Where(c => c.CourseId == course.CourseId).FirstOrDefaultAsync();
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
                result.ErrorMessage = e.Message;
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