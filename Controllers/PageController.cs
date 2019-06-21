using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Pegasus_backend.Controllers;
using Pegasus_backend.ActionFilter;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private IMapper _mapper;
        public PageController(pegasusContext.ablemusicContext pegasusContext, IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        // GET: api/Page
        [HttpGet]
        public async Task<IActionResult> GetGroup()
        {
            var result = new Result<Object>();
            try
            {
                result.Data = await _pegasusContext.Page
                .Include(p => p.PageGroup)
                .Select(p => new {
                    p.PageId,
                    p.PageName,
                    p.PageGroupId,
                    p.Icon,
                    p.Url,
                    p.DisplayOrder,
                    p.Para,
                    p.ParaFlag,
                    p.PageGroup.PageGroupName }
                )
                .ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }

        // POST: api/Page
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostGroup(Page page)
        {
            var result = new Result<object>();
            Page pages = new Page();
            try
            {
                await _pegasusContext.Page.AddAsync(page);
                await _pegasusContext.SaveChangesAsync();
                result.Data = "success";
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPage(int id,[FromBody] PageModel pagemodel)
        {
            var result = new Result<object>();
            try
            {
                var pages =await _pegasusContext.Page
                .Where(s => s.PageId == id).FirstOrDefaultAsync();
                if (pages == null)
                {
                return NotFound(DataNotFound(result));
                }
                _mapper.Map (pagemodel, pages);
                _pegasusContext.Update(pages);
                await _pegasusContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            result.Data = "Success!";
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(int id)
        {
            var result = new Result<object>();
            try
            {
                var pages = await _pegasusContext.Page
                .Where(s => s.PageId == id).FirstOrDefaultAsync();
            if (pages == null)
             {
                return NotFound(DataNotFound(result));
             }
            _pegasusContext.Remove(pages);
            await _pegasusContext.SaveChangesAsync();
            result.Data = "success";

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

