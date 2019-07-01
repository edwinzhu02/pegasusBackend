using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController: BasicController
    {
        public HolidayController(ablemusicContext ablemusicContext, ILogger<HolidayController> log) : base(ablemusicContext, log)
        {
        }
        
        //GET: http://localhost:5000/api/room/forCalendar
        // [Route("forCalendar")]
        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetHolidays()
        {
            Result<IEnumerable<Object>> result = new Result<IEnumerable<Object>>();
            try
            {
                result.Data  = await _ablemusicContext.Holiday.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpDelete("{id}")]
     public async Task<IActionResult> DeleteHoliday(short id)
        {
            var result = new Result<string>();
            try
            {
                var holiday = _ablemusicContext.Holiday.Where(s => s.HolidayId == id).FirstOrDefault();
                if (holiday == null)
                {
                    return NotFound(DataNotFound(result));
                }

                _ablemusicContext.Remove(holiday);
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

        [HttpPost]
     public async Task<IActionResult> AddHoliday(Holiday holiday)
        {
            var result = new Result<string>();            
            if ((holiday.HolidayDate == null )||(holiday.HolidayName == null )){
                throw new Exception("Holiday date and name is required!");
            }
            try
            {

            //var courseCategory = pegasusContext.CourseCategory    //.Where(c => c.CourseCategoryId == id).FirstOrDefaultAsync();

            var iholiday 
                =  _ablemusicContext.Holiday.Where(c=>c.HolidayDate.Date ==holiday.HolidayDate.Date).FirstOrDefault();
            if (iholiday==null){
                    _ablemusicContext.Add(holiday);
                    await _ablemusicContext.SaveChangesAsync();
                    result.Data = "success";
                }
            else
                {
                    iholiday.HolidayName= holiday.HolidayName;
                    iholiday.HolidayDate= holiday.HolidayDate;                    
                    _ablemusicContext.Update(iholiday);
                    await _ablemusicContext.SaveChangesAsync();
                    result.Data = "success";                
                }
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