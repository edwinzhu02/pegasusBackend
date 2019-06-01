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
            try
            {
                result.Data = await _ablemusicContext.Staff.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(short id)
        {
            var result = new Result<string>();
            try
            {
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.StaffId == id);
                if (staff == null)
                {
                    return NotFound(DataNotFound(result));
                }

                staff.IsActivate = 0;
                _ablemusicContext.Update(staff);
                await _ablemusicContext.SaveChangesAsync();
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