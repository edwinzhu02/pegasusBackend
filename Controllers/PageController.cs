using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : BasicController
    {
        public PageController(ablemusicContext ablemusicContext, ILogger<PageController> log) : base(ablemusicContext, log)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetRolePage()
        {
            var result = new Result<Object>();
            try
            {

                var item = await _ablemusicContext.Page.ToListAsync();
                result.Data = item;
                result.IsSuccess = true;
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
        public async Task<IActionResult> PostRolePage(Page page)
        {
            var result = new Result<Object>();
            Page pageExists = new Page();
            try
            {
                pageExists = await _ablemusicContext.Page.Where(s => s.PageName == page.PageName)
                .FirstOrDefaultAsync();
            }

            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (pageExists != null)
            {
                result.ErrorMessage = "The pagename is already exists";
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            try
            {
                await _ablemusicContext.Page.AddAsync(page);
                await _ablemusicContext.SaveChangesAsync();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRolePage(int id)
        {
            var result = new Result<Object>();
            Page pageExists = new Page();
            try
            {
                pageExists = await _ablemusicContext.Page
                .Where(s => s.PageId == id).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            if (pageExists == null)
            {
                result.ErrorMessage = "The page id is not exists";
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            try
            {
                _ablemusicContext.Page.Remove(pageExists);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {


                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> PutRolePage([FromBody] Page page)
        {
            var result = new Result<string>();
            Page pageExists = new Page();
            try
            {
                pageExists = await _ablemusicContext.Page
                .Where(s => s.PageId == page.PageId
                ).FirstOrDefaultAsync();
                if (pageExists != null)
                {   
                    pageExists.Icon = page.Icon;
                    pageExists.IsActivate = page.IsActivate;
                    pageExists.PageName = page.PageName;
                    pageExists.Para = page.Para;
                    pageExists.ParaFlag =page.ParaFlag;
                    pageExists.Url =page.Url;
                    pageExists.DisplayOrder = page.DisplayOrder;
                    await _ablemusicContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            return Ok(result);
        }

    }


}

