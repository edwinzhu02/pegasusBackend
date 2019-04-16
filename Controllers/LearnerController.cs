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
        public ActionResult<List<Learner>> GetLearner(string name)
        {
            
            return _pegasusContext.Learner.Where(s => s.FirstName.ToLower().Contains(name.ToLower())).ToList();
        }
        
        //GET: http://localhost:5000/api/learner
        [HttpGet]
        public async Task<ActionResult<List<Learner>>> GetLearners()
        {
            return await _pegasusContext.Learner.ToListAsync();
        }
    }
}