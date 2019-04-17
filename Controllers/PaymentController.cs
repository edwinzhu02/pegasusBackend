using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController: ControllerBase
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private Payment paymentItem;
        private LessonRemain _lessonRemain;

        public PaymentController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        //GET: http://localhost:5000/api/payment/invoice/:studentId
        [HttpGet]
        [Route("invoice/{id}")]
        public ActionResult<List<Invoice>> GetInvoice(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                return _pegasusContext.Invoice.Where(s => s.LearnerId == id).Include(s => s.Term)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }
        }
        
        
        //GET: http://localhost:5000/api/payment/payInvoice
        [HttpPost]
        [Route("payInvoice")]
        public IActionResult SavePaymentDetails([FromBody] InvoicePay details)
        {
            var invoiceItem = _pegasusContext.Invoice.
                FirstOrDefault(s => s.InvoiceId == details.InvoiceId & s.LearnerId == details.LearnerId);
            Result<string> result = new Result<string>();
            if (invoiceItem==null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Invoice does not found.";
                return BadRequest(result);
            }
            else
            {
                //Find updated item in Invoice table
                invoiceItem.PaidFee = invoiceItem.PaidFee + details.Amount;
                invoiceItem.OwingFee = invoiceItem.OwingFee - details.Amount;
                
                //business Logic
                if (invoiceItem.OwingFee > 0)
                {
                    invoiceItem.IsPaid = 0;
                }
                else
                {
                    invoiceItem.IsPaid = 1;
                }
                
                
                //initialize the Payment Item which need to add to payment Table
                try
                {
                    paymentItem = new Payment()
                    {
                        PaymentMethod = (byte) Models.PaymentMethod.GetPaymentMethod(details.PaymentMethod),
                        LearnerId = details.LearnerId,
                        Amount = details.Amount,
                        CreatedAt = DateTime.Now,
                        InvoiceId = details.InvoiceId,
                        StaffId = details.StaffId

                    };
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
                
                //initialize fund item in fund table
                var fundItem = _pegasusContext.Fund.FirstOrDefault(s => s.LearnerId == details.LearnerId);
                fundItem.Balance = fundItem.Balance + details.Amount;
                fundItem.UpdatedAt = DateTime.Now;
                
                //generate lesson_remain table
                /*DateTime date1 = new invoiceItem.EndDate.*/
                try
                {
                    _lessonRemain = new LessonRemain()
                    {
                        Quantity = invoiceItem.LessonQuantity,
                        TermId = invoiceItem.TermId,
                        ExpiryDate = invoiceItem.EndDate.Value.AddMonths(3),
                        CourseInstanceId = invoiceItem.CourseInstanceId,
                        LearnerId = invoiceItem.LearnerId
                    };
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
                

                
                try
                {
                    if (invoiceItem.IsPaid == 1)
                    {
                        _pegasusContext.Add(_lessonRemain);
                    }
                    
                    _pegasusContext.Add(paymentItem);
                    _pegasusContext.Update(invoiceItem);
                    _pegasusContext.Update(fundItem);
                    _pegasusContext.SaveChanges();
                    
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }

                result.IsSuccess = true;
                result.Data = "success";
                return Ok(result);



            }
            
        }
    }
}