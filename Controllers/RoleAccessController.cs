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
    public class RoleAccessController : BasicController
    {
        public RoleAccessController(ablemusicContext ablemusicContext, ILogger<PageController> log) : base(ablemusicContext, log)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleAccess()
        {
            var result = new Result<Object>();
            try
            {
                var getRole = await _ablemusicContext.RoleAccess.ToListAsync();
                result.Data = getRole;
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
        public async Task<IActionResult> PostRoleAccess(RoleAccess roleaccess)
        {
            var result = new Result<Object>();
            RoleAccess ra = new RoleAccess();


            try
            {
                await _ablemusicContext.RoleAccess.AddAsync(roleaccess);
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
public async Task<IActionResult> DeleteRoleAccess(int id)
        {
            var result = new Result<Object>();
            RoleAccess ra = new RoleAccess();
            try
            {
                ra = await _ablemusicContext.RoleAccess
                .Where(s => s.RoleAccessId == id).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            if (ra == null)
            {
                result.ErrorMessage = "The id is not exists";
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            try
            {
                _ablemusicContext.RoleAccess.Remove(ra);
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
        public async Task<IActionResult> PutRolePage([FromBody] RoleAccess roleaccess)
        {
            var result = new Result<string>();
            RoleAccess ra = new RoleAccess();
            try
            {
                ra = await _ablemusicContext.RoleAccess
                .Where(s => s.RoleAccessId == roleaccess.RoleAccessId
                ).FirstOrDefaultAsync();
                if (ra != null)
                {   
                    ra.IsMobile = roleaccess.IsMobile;
                    ra.PageId =roleaccess.PageId;
                    ra.RoleAccessId =roleaccess.RoleAccessId;
                    ra.RoleId = roleaccess.RoleId;
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