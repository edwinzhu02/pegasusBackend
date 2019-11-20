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
using Microsoft.Extensions.Logging;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BasicController
    {
        private IMapper _mapper;
        public PaymentController(ablemusicContext ablemusicContext, ILogger<PaymentController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        [HttpGet("[action]/{learnerId}")]
        public async Task<IActionResult> PaymentByLearner(int learnerId)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var payments = await _ablemusicContext.Payment
    .Where(d => d.LearnerId == learnerId)
     .Include(p => p.Invoice)
     .Include(p => p.Learner)
     .Include(p => p.SoldTransaction).ThenInclude(p => p.Product)
     .Include(t => t.Staff).OrderByDescending(p => p.CreatedAt).ToListAsync();
                //&& orgs.Contains(d.Staff.StaffOrg.FirstOrDefault().OrgId)
                result.Data = _mapper.Map<List<GetPaymentModel>>(payments);

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("[action]/{staffId}/{beginDate}/{endDate}")]
        public async Task<IActionResult> PaymentByDate(short staffId, DateTime beginDate, DateTime endDate)
        {
            Result<Object> result = new Result<object>();
            endDate = endDate.AddDays(1);
            try
            {
                var orgs = await _ablemusicContext.StaffOrg.Where(o => o.StaffId == staffId).Select(o => o.OrgId).ToListAsync();
                var payments = await _ablemusicContext.Payment
                    .Where(d => d.CreatedAt > beginDate && d.CreatedAt < endDate
                        && orgs.Contains(d.Staff.StaffOrg.FirstOrDefault().OrgId))
                     .Include(p => p.Invoice)
                     .Include(p => p.Learner)
                     .Include(p => p.SoldTransaction).ThenInclude(p => p.Product)
                     .Include(p => p.Staff).ThenInclude(s => s.StaffOrg)
                     .Include(t => t.Staff).OrderByDescending(p => p.CreatedAt).ToListAsync();
                //&& orgs.Contains(d.Staff.StaffOrg.FirstOrDefault().OrgId)
                result.Data = _mapper.Map<List<GetPaymentModel>>(payments);

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
        [CheckModelFilter]
        [HttpPost]
        [Route("payInvoice")]

        public async Task<IActionResult> SavePaymentDetails([FromBody] InvoicePay details)
        {
            Result<string> result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var invoiceItem =
                        await _ablemusicContext.Invoice.FirstOrDefaultAsync(s => s.InvoiceId == details.InvoiceId);
                    var learner = await _ablemusicContext.Learner.FirstOrDefaultAsync(s => s.LearnerId == details.LearnerId);
                    if (invoiceItem == null)
                    {
                        throw new Exception("Invoice does not found.");
                    }
                    if (learner == null)
                    {
                        throw new Exception("Student does not found.");
                    }
                    decimal creditAmt = (details.UseCredit ? learner.Credit : 0).Value;
                    //the owing fee for invoice cannot be negative.

                    if (invoiceItem.OwingFee - details.Amount - creditAmt < 0)
                    {
                        throw new Exception("You only need to pay " + (invoiceItem.OwingFee - creditAmt) + " dollar. No more than it");
                    }

                    invoiceItem.PaidFee = invoiceItem.PaidFee + details.Amount;
                    invoiceItem.OwingFee = invoiceItem.OwingFee - details.Amount - creditAmt;
                    invoiceItem.Credit = creditAmt;
                    if (invoiceItem.OwingFee > 0)
                    {
                        invoiceItem.IsPaid = 0;
                    }

                    if (invoiceItem.OwingFee == 0)
                    {
                        invoiceItem.IsPaid = 1;
                    }
                    _ablemusicContext.Update(invoiceItem);
                    await _ablemusicContext.SaveChangesAsync();
                    if (creditAmt > 0)
                    {
                        learner.Credit -= creditAmt;
                        _ablemusicContext.Update(learner);
                        await _ablemusicContext.SaveChangesAsync();
                    }
                    //save the Invoice payment to Payment table
                    var paymentItem = new Payment();
                    _mapper.Map(details, paymentItem);
                    paymentItem.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    paymentItem.PaymentType = 1;
                    paymentItem.IsConfirmed = 0;
                    if (paymentItem.Amount >= 0)
                        _ablemusicContext.Add(paymentItem);
                    await _ablemusicContext.SaveChangesAsync();

                    var fundItem =
                        await _ablemusicContext.Fund.FirstOrDefaultAsync(s => s.LearnerId == details.LearnerId);
                    if (fundItem == null)
                    {
                        var fundNewItem = new Fund();
                        fundNewItem.LearnerId = details.LearnerId;
                        fundNewItem.Balance = details.Amount;
                        fundNewItem.Balance = details.Amount;
                        fundNewItem.UpdatedAt = toNZTimezone(DateTime.UtcNow);
                        _ablemusicContext.Add(fundNewItem);
                    }
                    else
                    {
                        fundItem.Balance = fundItem.Balance + details.Amount;
                        fundItem.UpdatedAt = toNZTimezone(DateTime.UtcNow);
                        _ablemusicContext.Update(fundItem);
                    }
                    await _ablemusicContext.SaveChangesAsync();

                    if (invoiceItem.IsPaid == 1)
                    {
                        if (invoiceItem.CourseInstanceId != null)
                        { //if this is a one on one session 
                            //
                            var invoiceLessonRemain =
                                _ablemusicContext.LessonRemain.FirstOrDefault(s =>
                                    s.CourseInstanceId == invoiceItem.CourseInstanceId);
                            if (invoiceLessonRemain != null)
                            {
                                invoiceLessonRemain.Quantity =
                                    invoiceLessonRemain.Quantity + invoiceItem.LessonQuantity;
                                _ablemusicContext.Update(invoiceLessonRemain);
                                await _ablemusicContext.SaveChangesAsync();
                            }
                            else
                            {
                                var lessonRemain = new LessonRemain
                                {
                                    Quantity = invoiceItem.LessonQuantity,
                                    TermId = invoiceItem.TermId,
                                    ExpiryDate = invoiceItem.EndDate.Value.AddMonths(3),
                                    CourseInstanceId = invoiceItem.CourseInstanceId,
                                    LearnerId = invoiceItem.LearnerId
                                };
                                _ablemusicContext.Add(lessonRemain);
                                await _ablemusicContext.SaveChangesAsync();
                            }

                        }
                        //if  (invoiceItem.CourseInstanceId != null)
                        //await SaveLesson(details.InvoiceId,0,1);


                    }
                    var lesson = _ablemusicContext.Lesson
                    .Where(l => l.InvoiceNum == invoiceItem.InvoiceNum && l.IsTrial == 1).FirstOrDefault();
                    if (lesson != null)
                    {
                        lesson.IsPaid = 1;
                        _ablemusicContext.Update(lesson);
                        await _ablemusicContext.SaveChangesAsync();
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
        public async Task<IActionResult> SaveProdPayment([FromForm(Name = "paymentTranList")] string paymentTranList)
        {
            Result<string> result = new Result<string>();
            try
            {
                var paymentTranListJson = JsonConvert.DeserializeObject<PaymentTranModel>(paymentTranList);
                Payment payment = new Payment();
                _mapper.Map(paymentTranListJson, payment);
                payment.CreatedAt = toNZTimezone(DateTime.UtcNow);
                payment.PaymentType = 2;
                int i = 0;
                decimal? amount = 0;
                foreach (var detail in payment.SoldTransaction)
                {
                    var stock = await _ablemusicContext.Stock.FirstOrDefaultAsync(x => x.OrgId == paymentTranListJson.OrgId && x.ProductId == detail.ProductId);
                    var name = await _ablemusicContext.Product.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);
                    //return Ok(stock);
                    if (stock == null)
                    {
                        throw new Exception(name.ProductName + " is out of stock");
                    }
                    detail.BeforeQuantity = stock.Quantity;
                    detail.AflterQuantity = detail.BeforeQuantity - detail.SoldQuantity;
                    detail.LearnerId = paymentTranListJson.LearnerId;
                    detail.PaymentId = payment.PaymentId;

                    if (detail.AflterQuantity < 0)
                    {
                        throw new Exception(name.ProductName + " has not enough stock, only " + stock.Quantity + " left");
                    }
                    detail.StockId = stock.StockId;

                    detail.CreatedAt = toNZTimezone(DateTime.UtcNow);
                    detail.Amount = name.SellPrice * detail.SoldQuantity;
                    detail.DiscountedAmount = name.SellPrice * detail.SoldQuantity;
                    if (detail.DiscountAmount != 0)
                    {
                        detail.DiscountedAmount -= detail.DiscountAmount;
                    }
                    else if (detail.DiscountRate != 1)
                    {
                        if (detail.DiscountRate > 1)
                        {
                            throw new Exception("Discount Rate must less than 1");
                        }
                        detail.DiscountedAmount *= detail.DiscountRate;
                    }

                    stock.Quantity -= detail.SoldQuantity;
                    _ablemusicContext.Stock.Update(stock);
                    _mapper.Map(detail, payment.SoldTransaction.ToArray()[i]);
                    i++;
                    amount += detail.DiscountedAmount;

                }
                await _ablemusicContext.Payment.AddAsync(payment);

                if (Math.Round(amount.Value, 2) != payment.Amount)
                {
                    throw new Exception("Amount Error, payment amount is not equal to product price!");
                }
                if (payment.PaymentMethod == 4)
                {
                    AddToInvoice(payment,paymentTranListJson.InvoiceNum);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }

        private void AddToInvoice(Payment payment,string InvoiceNum)
        {
            long  ii = 0;
            var today = DateTime.UtcNow.ToNZTimezone();
            var termIds = _ablemusicContext.Term.
                Where(t => t.EndDate > today.Date).Select(t => t.TermId).ToList();

            if (termIds.Count == 0) throw new Exception("Sorry, Can not find a term");

            var invoice = new InvoiceWaitingConfirm();
            if (InvoiceNum!=null)
                invoice = _ablemusicContext.InvoiceWaitingConfirm.
                FirstOrDefault(i => i.InvoiceNum == InvoiceNum
                    && i.IsActivate == 1 && i.IsConfirmed == 0);
            else 
                invoice = _ablemusicContext.InvoiceWaitingConfirm.Where(i => termIds.Contains(i.TermId.Value)).
                FirstOrDefault(i => i.LearnerId == payment.LearnerId
                    && i.IsActivate == 1 && i.IsConfirmed == 0);

            if (invoice == null) throw new Exception("Sorry, Can not find a draft invoice");

            foreach (var tran in payment.SoldTransaction)
            {
                ii = getInvoiceItem(invoice);
                ii++;
                invoice.TotalFee += tran.DiscountedAmount;
                invoice.OwingFee += tran.DiscountedAmount;
                switch (ii)
                {
                    case 1:
                        invoice.Other9Fee = tran.DiscountedAmount;
                        invoice.Other9FeeName = tran.Product.ProductName;
                        break;
                    case 2:
                        invoice.Other10Fee = tran.DiscountedAmount;
                        invoice.Other10FeeName = tran.Product.ProductName;
                        break;
                    case 3:
                        invoice.Other11Fee = tran.DiscountedAmount;
                        invoice.Other11FeeName = tran.Product.ProductName;
                        break;
                    case 4:
                        invoice.Other12Fee = tran.DiscountedAmount;
                        invoice.Other12FeeName = tran.Product.ProductName;
                        break;
                    case 5:
                        invoice.Other13Fee = tran.DiscountedAmount;
                        invoice.Other13FeeName = tran.Product.ProductName;
                        break;
                    default:
                        throw new Exception("You Can Buy More Than 5 Books at a Time");
                }
                _ablemusicContext.InvoiceWaitingConfirm.Update(invoice);
            }
        }
        private short getInvoiceItem(InvoiceWaitingConfirm invoice){
            if (invoice.Other13Fee!= null && invoice.Other13Fee!= 0 ) return 5;
            if (invoice.Other12Fee!= null && invoice.Other12Fee!= 0 ) return 4;
            if (invoice.Other11Fee!= null && invoice.Other11Fee!= 0 ) return 3;
            if (invoice.Other10Fee!= null && invoice.Other10Fee!= 0 ) return 2;
            if (invoice.Other9Fee!= null && invoice.Other9Fee!= 0 ) return 1;            
            return 0;
        }
        [HttpPut("{paymentId}/{comment}")]
        public async Task<IActionResult> Put(int paymentId, string comment)
        {
            var result = new Result<Payment>();
            var payment = new Payment();
            try
            {
                payment = await _ablemusicContext.Payment.Where(p => p.PaymentId == paymentId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (payment == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "payment not found";
                return BadRequest(result);
            }
            payment.IsConfirmed = 1;
            payment.Comment = comment;
            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = payment;
            return Ok(result);
        }
    }
}