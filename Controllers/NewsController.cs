using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : BasicController
    {

        public NewsController(ablemusicContext ablemusicContext, ILogger<LookupsController> log) : base(ablemusicContext, log)
        {
        }

        [HttpPost]
        
        public async Task<IActionResult> Post([FromBody]News news)
        {
            Result<string> result = new Result<string>();
            try
            {
                await _ablemusicContext.AddAsync(news);
                await _ablemusicContext.SaveChangesAsync();
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
        [HttpGet("{begin}/{take}")]
        public async Task<IActionResult> GetNews(int begin, int take)
        {
            Result<List<News>> result = new Result<List<News>>();
            try
            {
                result.Data = await _ablemusicContext.News.OrderByDescending(n => n.IsTop).
                        ThenByDescending(n => n.CreatedAt).Skip(begin).Take(take).ToListAsync();
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
                result.ErrorMessage = "not found";
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