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
    public class PageController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;

        public PageController(pegasusContext.ablemusicContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetRolePage()
        {
            var result = new Result<Object>();
            try
            {
                
                var item = await _pegasusContext.RoleAccess
                    .Include(s=>s.Page)
                    .Select(s=> new {s.RoleId,s.Page.Url})
                    .ToListAsync();
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
    }
}