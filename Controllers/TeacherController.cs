using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using AutoMapper;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {

        private readonly pegasusContext.pegasusContext _pegasusContext;

        public TeacherController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }

        //GET: http://localhost:5000/api/teacher
        [HttpGet]
        public async Task<ActionResult> GetTeacher()
        {
            Result<List<Teacher>> result = new Result<List<Teacher>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.Teacher
                    .Include(s=>s.TeacherLanguage)
                    .ThenInclude(s=>s.Lang)
                    .Include(s=>s.TeacherQualificatiion)
                    .ThenInclude(s=>s.Quali)
                    .Include(s=>s.TeacherCourse)
                    .ThenInclude(s=>s.Course)
                    .ToListAsync();
                return Ok(result);
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