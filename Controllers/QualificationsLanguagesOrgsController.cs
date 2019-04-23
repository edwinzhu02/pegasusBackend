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
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QualificationsLanguagesOrgsController: BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public QualificationsLanguagesOrgsController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        
        //GET: http://localhost:5000/api/qualificationslanguagesorgs
        [HttpGet]
        public ActionResult GetDetailsForTeacher()
        {
            Result<DetailsForTeacherRegister> result = new Result<DetailsForTeacherRegister>();
            try
            {
                DetailsForTeacherRegister details = new DetailsForTeacherRegister()
                {
                    qualifications = _pegasusContext.Qualification.ToList(),
                    Languages = _pegasusContext.Language.ToList(),
                    Orgs = _pegasusContext.Org.ToList()
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