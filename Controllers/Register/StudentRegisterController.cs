using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Parent = Pegasus_backend.Models.Parent;

namespace Pegasus_backend.Controllers.Register
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentRegisterController : BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;
        public StudentRegisterController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        //POST: http://localhost:5000/api/studentregister
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> StudentRegister([FromForm] IList<IFormFile> image, [FromForm] IList<IFormFile> ABRSM,[FromForm] string details)
        {
            Result<string> result = new Result<string>();
            try
            {
                
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<StudentRegister>(details);
                    var newLearner = new Learner();
                    _mapper.Map(detailsJson, newLearner);
                    _pegasusContext.Add(newLearner);
                    await _pegasusContext.SaveChangesAsync();

                    if (image.Count != 0)
                    {
                        newLearner.Photo = $"images/LearnerImages/{ContentDispositionHeaderValue.Parse(image[0].ContentDisposition).FileName.Trim('"')}";
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(image[0],"image");
                    }

                    if (ABRSM.Count != 0)
                    {
                        newLearner.G5Certification = $"images/ABRSM_Grade5_Certificate/{ContentDispositionHeaderValue.Parse(ABRSM[0].ContentDisposition).FileName.Trim('"')}";
                        newLearner.IsAbrsmG5 = 1;
                        _pegasusContext.Update(newLearner);
                        await _pegasusContext.SaveChangesAsync();
                        UploadFile(ABRSM[0],"ABRSM");
                    }
                    
                    dbContextTransaction.Commit();
                }
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
        
    }
}
