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
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QualificationsLanguagesOrgsController: BasicController
    {
        public QualificationsLanguagesOrgsController(ablemusicContext ablemusicContext, ILogger<QualificationsLanguagesOrgsController> log) : base(ablemusicContext, log)
        {
        }
        
        
        //GET: http://localhost:5000/api/qualificationslanguagesorgs
        [HttpGet]
        public async Task<IActionResult> GetDetailsForTeacher()
        {
            Result<DetailsForTeacherRegister> result = new Result<DetailsForTeacherRegister>();
            try
            {
                DetailsForTeacherRegister details = new DetailsForTeacherRegister()
                {
                    qualifications = await _ablemusicContext.Qualification.ToListAsync(),
                    Languages = await _ablemusicContext.Language.ToListAsync(),
                    Orgs = await _ablemusicContext.Org.ToListAsync()
                };
                result.Data = details;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}