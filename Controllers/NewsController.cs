using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
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
                _mapper.Map(news,newItem);
                var newsData = Encoding.ASCII.GetBytes(news.newsData);
                newItem.NewsData = newsData;
                newItem.CreatedAt = DateTime.UtcNow.AddHours(12);
                await _ablemusicContext.AddAsync(newItem);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = "news created successfully";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        }
    }
}