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
    public class RegisterController: ControllerBase
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public RegisterController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }

        [HttpPost]
        public ActionResult<string> Register([FromBody] Register details)
        {
            return "";
        }
        
        
    }
}