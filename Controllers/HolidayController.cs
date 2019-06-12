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


namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;

        public HolidayController(pegasusContext.ablemusicContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
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
                result.Data  = await _pegasusContext.Holiday.ToListAsync();
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
                var holiday = _pegasusContext.Holiday.FirstOrDefault(s => s.HolidayId == id);
                if (holiday == null)
                {
                    return NotFound(DataNotFound(result));
                }

                _pegasusContext.Remove(holiday);
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

        [HttpPost]
     public async Task<IActionResult> AddHoliday(Holiday holiday)
        {
            var result = new Result<string>();            
            if ((holiday.HolidayDate == null )||(holiday.HolidayName == null )){
                result.IsSuccess = false;
                result.ErrorMessage = "Holiday date and name is required!";
                return BadRequest(result);                
            }
            try
            {

            //var courseCategory = pegasusContext.CourseCategory    //.Where(c => c.CourseCategoryId == id).FirstOrDefaultAsync();
                var kholiday = _pegasusContext.Holiday.FirstOrDefault(s => s.HolidayId == 1);
            var iholiday 
                =  _pegasusContext.Holiday.Where(c=>c.HolidayDate.Date ==holiday.HolidayDate.Date).FirstOrDefault();
            if (iholiday==null){
                    _pegasusContext.Add(holiday);
                    await _pegasusContext.SaveChangesAsync();
                    result.Data = "success";
                }
            else
                {
                    iholiday.HolidayName= holiday.HolidayName;
                    iholiday.HolidayDate= holiday.HolidayDate;                    
                    _pegasusContext.Update(iholiday);
                    await _pegasusContext.SaveChangesAsync();
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