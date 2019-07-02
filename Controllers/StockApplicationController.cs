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
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class StockApplicationController : BasicController
    {
        public StockApplicationController(ablemusicContext ablemusicContext, ILogger<RemindLogController> log) : base(ablemusicContext, log)
        {
        }
        // GET: api/StockApplication

        [HttpGet("{beginDate}/{endDate}")]
        public async Task<IActionResult> GetStockApp(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<Object>();
            try
            {
                result.Data = await _ablemusicContext.StockApplication
                .Where(s => s.ApplyAt >= beginDate && s.ApplyAt <= endDate).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        // DELETE: api/StockApplication/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteStockApp(short id)
        // {
        //     var result = new Result<string>();
        //     try
        //     {
        //         var dStockApp = await _ablemusicContext.StockApplication
        //         .Where(s => s.ApplicationId == id).FirstOrDefaultAsync();
        //         if (dStockApp == null)
        //         {
        //             return NotFound(DataNotFound(result));
        //         }
        //         _ablemusicContext.Remove(dStockApp);
        //         await _ablemusicContext.SaveChangesAsync();
        //         result.Data = "success";
        //     }
        //     catch (Exception ex)
        //     {
        //         result.IsSuccess = false;
        //         result.ErrorMessage = ex.Message;
        //         return BadRequest(result);
        //     }
        //     return Ok(result);
        // }


        // // POST: api/StockApplication
        // [HttpPost]
        // public async Task<IActionResult> ApplyStock(StockApplication stockApplication)
        // {
        //     var result = new Result<string>();
        //     try
        //     {
        //         var pApplyStock = await _ablemusicContext.StockApplication
        //         .Include(s => s.ApplicationDetails).FirstOrDefaultAsync();
        //         _ablemusicContext.Update(pApplyStock.ProcessStatus == 1);
        //         _ablemusicContext.Add(pApplyStock);
        //         await _ablemusicContext.SaveChangesAsync();

        //     }
        //     catch (Exception ex)
        //     {
        //         result.IsSuccess = false;
        //         result.ErrorMessage = ex.Message;
        //         return BadRequest(result);
        //     }

        //     return Ok(result);

        // }

        // [HttpPut]
        // public async Task<IActionResult> UpdateProcess2(DateTime beginDate, DateTime endDate, StockApplication stockapplication, ApplicationDetails applicationdetails)
        // {
        //     var result = new Result<object>();
        //     try
        //     {
        //         var updateP2 = await _ablemusicContext.StockApplication
        //         .Where(s => s.ApplyAt >= beginDate && s.ApplyAt <= endDate)
        //         .Include(d => d.ApplicationDetails).FirstOrDefaultAsync();
        //         if (updateP2 == null)
        //         {
        //             return NotFound(DataNotFound(result));
        //         }
        //         updateP2.ReplyAt = stockapplication.ReplyAt;
        //         updateP2.ReplyContent = stockapplication.ReplyContent;
        //         _ablemusicContext.Update(updateP2);
        //         await _ablemusicContext.SaveChangesAsync();
        //         result.Data ="Success!";

    
        // }
        //     catch (Exception e)
        //     {
        //         result.ErrorMessage = e.Message;
        //         result.IsSuccess = false;
        //         return BadRequest(result);
        //     }
        //     return Ok(result);
        // }

        // [HttpPut]
        // public async Task<IActionResult> UpdateProcess3(DateTime beginDate, DateTime endDate, StockApplication stockapplication, ApplicationDetails applicationdetails)
        // {
        //     var result = new Result<object>();
        //     try
        //     {
        //         var updateP3 = await _ablemusicContext.StockApplication
        //         .Where(s => s.ApplyAt >= beginDate && s.ApplyAt <= endDate)
        //         .Include(d => d.ApplicationDetails).FirstOrDefaultAsync();
        //         if (updateP3 == null)
        //         {
        //             return NotFound(DataNotFound(result));
        //         }
        //         updateP3.DeliverAt = stockapplication.DeliverAt;
        //         updateP3.ApplicationDetails.FirstOrDefault().DeliveredQty = applicationdetails.DeliveredQty;
        //         _ablemusicContext.Update(updateP3);
        //         await _ablemusicContext.SaveChangesAsync();
        //         result.Data ="Success!";
    
        // }
        //     catch (Exception e)
        //     {
        //         result.ErrorMessage = e.Message;
        //         result.IsSuccess = false;
        //         return BadRequest(result);
        //     }
        //     return Ok(result);
        // }

        // [HttpPut]
        // public async Task<IActionResult> UpdateProcess4(DateTime beginDate, DateTime endDate, StockApplication stockapplication, ApplicationDetails applicationdetails)
        // {
        //     var result = new Result<object>();
        //     try
        //     {
        //         var updateP4 = await _ablemusicContext.StockApplication
        //         .Where(s => s.ApplyAt >= beginDate && s.ApplyAt <= endDate)
        //         .Include(d => d.ApplicationDetails).FirstOrDefaultAsync();
        //         if (updateP4 == null)
        //         {
        //             return NotFound(DataNotFound(result));
        //         }
        //         updateP4.RecieveAt = stockapplication.RecieveAt;
        //         updateP4.ApplicationDetails.FirstOrDefault().ReceivedQty = applicationdetails.ReceivedQty;
        //         _ablemusicContext.Update(updateP4);
        //         await _ablemusicContext.SaveChangesAsync();
        //         result.Data ="Success!";
    
        // }
        //     catch (Exception e)
        //     {
        //         result.ErrorMessage = e.Message;
        //         result.IsSuccess = false;
        //         return BadRequest(result);
        //     }
        //     return Ok(result);
        // }
    }
}