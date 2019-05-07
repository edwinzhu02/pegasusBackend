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
        public async Task<IActionResult> SaveProdPayment(IEnumerable <PaymentTranModel> paymentTranList )
        {

            var result = new Result<IEnumerable<PaymentTranModel>>();
            var tranDetails = paymentTranList.ToArray()[0].SoldTransaction.AsQueryable();

            var paymentDetail = paymentTranList.ToArray()[0];

            Payment payment = new Payment();
            _mapper.Map(paymentDetail, payment);
            payment.CreatedAt = DateTime.Now;

            int i = 0;
            foreach (var detail in tranDetails)
            {
                var stock = await _pegasusContext.Stock.FirstOrDefaultAsync(x => x.OrgId == paymentTranList.ToList()[0].OrgId && x.ProductId == detail.ProductId);
                detail.BeforeQuantity = stock.Quantity;
                detail.AflterQuantity = detail.BeforeQuantity - detail.SoldQuantity;
                detail.LearnerId = paymentTranList.ToList()[0].LearnerId;
                detail.PaymentId = payment.PaymentId;
                var name = await _pegasusContext.Product.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);
                if (detail.AflterQuantity < 0)
                {
                    throw new Exception(name.ProductName + " has not enough stock, only " + stock.Quantity + " left");
                }
                detail.StockId = stock.StockId;
                detail.CreatedAt = DateTime.Now;
                if (detail.DiscountAmount != null)
                {
                    detail.DiscountedAmount = name.SellPrice * detail.SoldQuantity - detail.DiscountAmount;
                }
                else if (detail.DiscountRate != null)
                {
                    detail.DiscountedAmount = name.SellPrice * detail.SoldQuantity * detail.DiscountRate;
                }
                stock.Quantity -= detail.SoldQuantity;
                _pegasusContext.Stock.Update(stock);
                _mapper.Map(detail, payment.SoldTransaction.ToArray()[i]);
                i++;

            }
            try
            {
                await _pegasusContext.Payment.AddAsync(payment);
                await _pegasusContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsFound = false;
            }
            //result.Data = paymentTranList;

            return Ok(result);
        }


    }
}