using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerController: ControllerBase
    {
        
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public LearnerController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        //GET: http://localhost:5000/api/learner/:name
        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> GetLearner(string name)
        {
            Result<IEnumerable<Learner>> result = new Result<IEnumerable<Learner>>();
            try
            {
                result.Data = await _pegasusContext.Learner.Where(s =>s.FirstName.Contains(name)).ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);


        }
        
        //GET: http://localhost:5000/api/learner
        [HttpGet]
        public async Task<ActionResult<List<Learner>>> GetLearners()
        {
            Result<List<Learner>> result = new Result<List<Learner>>();
            try
            {
                var data = await _pegasusContext.Learner.ToListAsync();
                result.Data = data;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}