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
    public class LookupsController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public LookupsController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            Result<List<Lookup>> result = new Result<List<Lookup>>();
            try
            {
                result.Data = await _ablemusicContext.Lookup.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Lookups/5
        [HttpGet("{type}")]
        public async Task<IActionResult> GetLookup([FromRoute] int type)
        {
            Result<List<Lookup>> result = new Result<List<Lookup>>();
            try
            {
                result.Data = await _ablemusicContext.Lookup.Where(l => l.LookupType == type).ToListAsync();
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
                result.ErrorMessage = "Type not found";
                return NotFound(result);
            }

            return Ok(result);
        }
       
        private bool LookupExists(int id)
        {
            return _ablemusicContext.Lookup.Any(e => e.LookupId == id);
        }
    }
}