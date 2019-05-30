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
    public class StaffController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public StaffController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffs()
        {
            var result = new Result<Object>();
            return Ok();
        }
    }
}