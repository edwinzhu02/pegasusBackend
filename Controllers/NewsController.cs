using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
=======
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
>>>>>>> 3655ae8eeaf7f53e4c8857a4c5ca04efce9ad8ef

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController: BasicController
    {
        private readonly IMapper _mapper;
        public NewsController(ablemusicContext ablemusicContext, ILogger<QualificationsLanguagesOrgsController> log,IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
            var result = new Result<object>();
            try
            {
                var news = await _ablemusicContext.News.ToListAsync();
                result.Data = news;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> PostNews([FromBody] NewsModel news)
        {
            var result = new Result<string>();
            try
            {
                var newItem = new News();
                _mapper.Map(newItem, news);
                newItem.CreatedAt = DateTime.UtcNow.AddHours(12);
                _ablemusicContext.Add(newItem);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = "news created successfully";
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