using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Pegasus_backend.ActionFilter;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController: ControllerBase
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private IMapper _mapper;
        public PaymentController(pegasusContext.pegasusContext pegasusContext,IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
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
                return BadRequest(result);
            }
        }
        
        //POST: http://localhost:5000/api/payment/payInvoice

        [HttpPost]
        [Route("payInvoice")]
        [CheckModelFilter]
        public async Task<IActionResult> SavePaymentDetails([FromBody] InvoicePay details)
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _pegasusContext.Database.BeginTransaction())
                {
                    var invoiceItem =
                        await _pegasusContext.Invoice.FirstOrDefaultAsync(s => s.InvoiceId == details.InvoiceId);
                    if (invoiceItem == null)
                    {
                        throw new Exception("Invoice does not found.");
                    }

                    //the owing fee for invoice cannot be negative.
                    if (invoiceItem.OwingFee - details.Amount < 0)
                    {
                        throw new Exception("You only need to pay " + invoiceItem.OwingFee + " dollar. No more than it");
                    }

                    invoiceItem.PaidFee = invoiceItem.PaidFee + details.Amount;
                    invoiceItem.OwingFee = invoiceItem.OwingFee - details.Amount;
                    if (invoiceItem.OwingFee > 0)
                    {
                        invoiceItem.IsPaid = 0;
                    }

                    if (invoiceItem.OwingFee == 0)
                    {
                        invoiceItem.IsPaid = 1;
                    }
                    _pegasusContext.Update(invoiceItem);
                    await _pegasusContext.SaveChangesAsync();

                    //save the Invoice payment to Payment table
                    var paymentItem = new Payment();
                    _mapper.Map(details, paymentItem);
                    paymentItem.CreatedAt = DateTime.Now;
                    paymentItem.PaymentType = 1;
                    _pegasusContext.Add(paymentItem);
                    await _pegasusContext.SaveChangesAsync();

                    var fundItem =
                        await _pegasusContext.Fund.FirstOrDefaultAsync(s => s.LearnerId == details.LearnerId);
                    fundItem.Balance = fundItem.Balance + details.Amount;
                    fundItem.UpdatedAt = DateTime.Now;
                    _pegasusContext.Update(fundItem);
                    await _pegasusContext.SaveChangesAsync();

                    if (invoiceItem.IsPaid == 1)
                    {
                        var lessonRemain = new LessonRemain
                        {
                            Quantity = invoiceItem.LessonQuantity,
                            TermId = invoiceItem.TermId,
                            ExpiryDate = invoiceItem.EndDate.Value.AddMonths(3),
                            CourseInstanceId = invoiceItem.CourseInstanceId,
                            LearnerId = invoiceItem.LearnerId
                        };
                        _pegasusContext.Add(lessonRemain);
                        await _pegasusContext.SaveChangesAsync();
                    }
                    dbContextTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            result.Data = "Success!";
            return Ok(result);
        }


        [HttpPost]
        [Route("[action]")]
        [CheckModelFilter]
        public async Task<IActionResult> SaveProdPayment([FromForm (Name = "paymentTranList")] string paymentTranList)
        {
            Result<string> result = new Result<string>();
            try{
                var paymentTranListJson = JsonConvert.DeserializeObject<PaymentTranModel>(paymentTranList);
                Payment payment = new Payment();
                _mapper.Map(paymentTranListJson, payment);
                payment.CreatedAt = DateTime.Now;
                payment.PaymentType = 2;
                int i = 0;
                foreach (var detail in payment.SoldTransaction)
                {
                    var stock = await _pegasusContext.Stock.FirstOrDefaultAsync(x => x.OrgId == paymentTranListJson.OrgId && x.ProductId == detail.ProductId);
                    detail.BeforeQuantity = stock.Quantity;
                    detail.AflterQuantity = detail.BeforeQuantity - detail.SoldQuantity;
                    detail.LearnerId = paymentTranListJson.LearnerId;
                    detail.PaymentId = payment.PaymentId;
                    var name = await _pegasusContext.Product.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);
                    if (detail.AflterQuantity < 0)
                    {
                        throw new Exception(name.ProductName + " has not enough stock, only " + stock.Quantity + " left");
                    }
                    detail.StockId = stock.StockId;
                    detail.CreatedAt = DateTime.Now;
                    detail.DiscountedAmount = name.SellPrice * detail.SoldQuantity;
                    if (detail.DiscountAmount != 0)
                    {
                        detail.DiscountedAmount -= detail.DiscountAmount;
                    }
                    else if (detail.DiscountRate != 0)
                    {
                        if(detail.DiscountRate >= 1)
                        {
                            throw new Exception("Discount Rate must less than 1");
                        }
                        detail.DiscountedAmount *= detail.DiscountRate;
                    }
                          
                    stock.Quantity -= detail.SoldQuantity;
                    _pegasusContext.Stock.Update(stock);
                    _mapper.Map(detail, payment.SoldTransaction.ToArray()[i]);
                    i++;

                }
                    await _pegasusContext.Payment.AddAsync(payment);
                    await _pegasusContext.SaveChangesAsync();
                    
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