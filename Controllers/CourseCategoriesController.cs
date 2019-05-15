using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseCategoriesController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public CourseCategoriesController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseCategory()
        {
            Result<List<CourseCategory>> result = new Result<List<CourseCategory>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await (from c in _ablemusicContext.CourseCategory
                                     select new CourseCategory
                                     {
                                         CourseCategoryId = c.CourseCategoryId,
                                         CourseCategoryName = c.CourseCategoryName,
                                         Course = null
                                     }).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }
            if(result.Data == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "Course Category not found";
                return NotFound(result);
            }
            return Ok(result);
        }
        
        private bool CourseCategoryExists(short id)
        {
            return _ablemusicContext.CourseCategory.Any(e => e.CourseCategoryId == id);
        }
    }
}