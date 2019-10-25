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
    public class CashBoxController : BasicController
    {
        private readonly IMapper _mapper;

        public CashBoxController(ablemusicContext ablemusicContext, ILogger<ProductController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }


        //GET: http://localhost:5000/api/product
        [HttpGet("{orgId}/{date}")]
        public async Task<ActionResult> GetLastCashBox(short orgId,DateTime date)
        {
            Result<Object> result = new Result<object>();
            //var payments =  new List<Payment>();
            try
            {
                short?[] staffOrg = await _ablemusicContext.StaffOrg.Where(s => s.OrgId == orgId).
                            Select(s => s.StaffId).ToArrayAsync();
                Payment[] payments = await _ablemusicContext.Payment.Where(p => staffOrg.Contains(p.StaffId)
                             && p.CreatedAt.Value.Date == date.Date).ToArrayAsync();
                decimal yestodayCashBox = getLastCashBox(orgId, staffOrg,date);
                result.Data =  yestodayCashBox;  
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
    
        [HttpPost("{orgId}/{staffId}/{cashOut}/{date}")]
        [CheckModelFilter]
        public async Task<ActionResult> PostCashBox(short orgId,short staffId, decimal cashOut,DateTime date)
        {
            Result<Object> result = new Result<object>();
            //var payments =  new List<Payment>();
            // dynamic cashBox = new Object();
            CashBox cashBox = new CashBox();
            try
            {
                short?[] staffOrg = await _ablemusicContext.StaffOrg.Where(s => s.OrgId == orgId).
                            Select(s => s.StaffId).ToArrayAsync();
                Payment[] payments = await _ablemusicContext.Payment.Where(p => staffOrg.Contains(p.StaffId)
                             && p.CreatedAt.Value.Date == date.Date).ToArrayAsync();
                var cashBoxCheck =await _ablemusicContext.CashBox.Where(p => orgId == p.OrgId
                             && p.CloseTime.Value.Date == date.Date).FirstOrDefaultAsync();                             
                if (cashBoxCheck!= null){
                    throw new Exception("Daily Log Has Already Submitted");
                }
                decimal yestodayCashBox ;
                yestodayCashBox = getLastCashBox(orgId, staffOrg,date);
                CashBoxInit(ref cashBox, orgId,staffId, cashOut, yestodayCashBox);
                paymentToCashBox(ref cashBox, payments);
                setCashBoxToday(ref cashBox);
                await _ablemusicContext.AddAsync(cashBox);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = cashBox;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        private void CashBoxInit(ref CashBox cashBox, short orgId,short staffId, decimal cashOut, decimal yestodayCashBox)
        {
            cashBox.InCash = 0;
            cashBox.Cheque = 0;
            cashBox.BankDepoist = 0;
            cashBox.Cheque = 0;
            cashBox.Eftpos = 0;
            cashBox.OutCash = cashOut;
            cashBox.YesterdayCash = yestodayCashBox;
            cashBox.OrgId = orgId;
            cashBox.StaffId = staffId;
            cashBox.CloseTime = toNZTimezone(DateTime.UtcNow);
            cashBox.CashBoxDate = toNZTimezone(DateTime.UtcNow).Date;
            
        }
        private void setCashBoxToday(ref CashBox cashBox)
        {
            
            cashBox.TodayCash = cashBox.YesterdayCash + cashBox.InCash - cashBox.OutCash;
        }
        private decimal getLastCashBox(short orgId, short?[] staffOrg,DateTime date)
        {
            //find last cash payment date
            DateTime? paymentDate = _ablemusicContext.Payment.Where(p => staffOrg.Contains(p.StaffId)
                       && p.CreatedAt.Value.Date < date.Date
                       && p.PaymentMethod == 1).Max(p => p.CreatedAt);
            var yestodayCashBox = _ablemusicContext.CashBox.Where(c => c.OrgId == orgId
                       && c.CashBoxDate == paymentDate.Value.Date).FirstOrDefault();
            if (yestodayCashBox == null)
                throw new Exception("Exists No Completed Dailylog Data!");

            return yestodayCashBox.TodayCash.Value;
        }
        private void paymentToCashBox(ref CashBox cashBox, Payment[] payments)
        {

            foreach (var payment in payments)
            {
                if (payment.PaymentMethod == 1) //cash
                {
                    cashBox.InCash += payment.Amount;
                }
                else if (payment.PaymentMethod == 2) //EftPos
                {
                     cashBox.Eftpos +=payment.Amount;
                }
                else if (payment.PaymentMethod == 3)  //online Transfer
                {
                    cashBox.BankDepoist += payment.Amount;
                }
                else
                {
                    cashBox.Cheque += payment.Amount;
                };
            }
        }
    }
}
