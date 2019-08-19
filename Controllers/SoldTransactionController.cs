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
    public class SoldTranscationController : BasicController
    {
    public SoldTranscationController(ablemusicContext ablemusicContext, ILogger<SoldTranscationController> log) : base(ablemusicContext, log)
        {
        }
        //GET: http://localhost:5000/api/SoldTranscation
        [HttpGet("{staffId}/{beginDate}/{endDate}")]
        public async Task <IActionResult> GetSold(short staffId, DateTime beginDate,DateTime endDate)
        {
            var result = new Result<Object>();
            var orgs = await _ablemusicContext.StaffOrg.Where(o=>o.StaffId==staffId).Select(o=>o.OrgId).ToListAsync();
            try 
            {
                result.Data = await _ablemusicContext.Payment
                .Include(s => s.SoldTransaction)
                .ThenInclude(p => p.Product)
                .Include(s => s.Staff)
                .Include(s => s.Learner).Where(s => s.PaymentType==2 
                    && s.CreatedAt.Value.Date>=beginDate.Date && s.CreatedAt.Value.Date<=endDate.Date
                    && orgs.Contains(s.Staff.StaffOrg.FirstOrDefault().OrgId) )
                .Select(s => new 
                {   s.LearnerId,
                    Learn = (s.Learner.FirstName + s.Learner.LastName),
                    s.StaffId,
                    Staff = (s.Staff.FirstName + s.Staff.LastName),
                    s.PaymentId,
                    s.PaymentMethod,
                    SoldTransaction = s.SoldTransaction.Select(st => new { st.Product.ProductName, st.Product.ProductId,
                    st.SoldQuantity,st.Amount,st.DiscountAmount,st.DiscountedAmount,st.DiscountRate
                    }),
                    s.Amount,
                    s.CreatedAt}).ToListAsync();
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
