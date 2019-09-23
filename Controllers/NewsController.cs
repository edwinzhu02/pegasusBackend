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
using MySqlX.XDevAPI.Common;

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
                var news = await _ablemusicContext.News
                    .OrderBy(s=>s.IsTop)
                    .ThenByDescending(s=>s.CreatedAt)
                    .Select(s=>new
                    {
                        s.UserId,s.NewsTitle,s.NewsType,s.Categroy,s.CreatedAt,s.IsTop,
                        NewsData = Encoding.ASCII.GetString(s.NewsData)
                    })
                    .ToListAsync();
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

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadTitlePhoto([FromForm(Name = "photo")] IFormFile photo)
        {
            var result = new Result<string>();
            try
            {
                if (photo == null)
                {
                    throw new Exception("Photo is null");
                }

                var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");
                var uploadResult = UploadFile(photo, "news/titlePhoto/", 1, strDateTime);
                if (!uploadResult.IsUploadSuccess)
                {
                    throw new Exception(uploadResult.ErrorMessage);
                }

               
                result.Data =  $"images/news/titlePhoto/{1 + strDateTime + Path.GetExtension(photo.FileName)}";;
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
                var newsData = Encoding.ASCII.GetBytes(news.newsData);
                var newItem = new News();
                _mapper.Map(news,newItem);
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