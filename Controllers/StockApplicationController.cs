using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using AutoMapper;
using Pegasus_backend.ActionFilter;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class StockApplicationController : BasicController
    {
        private readonly IMapper _mapper;
        public StockApplicationController(ablemusicContext ablemusicContext, IMapper mapper, ILogger<RemindLogController> log) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        // GET: api/StockApplication
        [HttpGet("{beginDate}/{endDate}")]
        public async Task<IActionResult> GetStockApp(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<object>();
            try
            {
                result.Data = await _ablemusicContext.StockApplication
                .Include(s => s.ApplyStaff)
                .Include(s => s.Org)
                .Include(s => s.ApplicationDetails)
                .ThenInclude(s => s.Product)
                .ThenInclude(s => s.ProdType)
                .ThenInclude(s => s.ProdCat)
                .Where(s => s.ApplyAt >= beginDate && s.ApplyAt <= endDate)
                .Select(s => new
                {
                    s.OrgId,
                    s.ApplyStaffId,
                    s.ApplyAt,
                    s.ApplicationId,
                    s.ProcessStatus,
                    s.ApplyReason,
                    s.ReplyContent,
                    s.IsDisputed,
                    s.ReplyAt,
                    s.RecieveAt,
                    s.DeliverAt,
                    s.Org,
                    s.ApplyStaff,
                    ApplicationDetails = s.ApplicationDetails.Select(ad => new
                    {
                        ad.ApplicationId,
                        ad.DetaillsId,
                        ad.ProductId,
                        ad.AppliedQty,
                        ad.DeliveredQty,
                        ad.ReceivedQty,
                        ad.Product,
                        ad.Product.ProdType,
                        ad.Product.ProdType.ProdCat,
                    })
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(result.Data == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Stock Application not found";
                return BadRequest(result);
            }
            return Ok(result);
        }

        // GET: api/StockApplication/getByProcesseStatus/1
        [HttpGet("getByProcesseStatus/{processStatus}")]
        public async Task<IActionResult> GetByProcesseStatus(short processStatus)
        {
            var result = new Result<object>();
            try
            {
                result.Data = await _ablemusicContext.StockApplication
                .Include(s => s.ApplyStaff)
                .Include(s => s.Org)
                .Include(s => s.ApplicationDetails)
                .ThenInclude(s => s.Product)
                .ThenInclude(s => s.ProdType)
                .ThenInclude(s => s.ProdCat)
                .Where(s => s.ProcessStatus == processStatus)
                .Select(s => new
                {
                    s.OrgId,
                    s.ApplyStaffId,
                    s.ApplyAt,
                    s.ApplicationId,
                    s.ProcessStatus,
                    s.ApplyReason,
                    s.ReplyContent,
                    s.IsDisputed,
                    s.ReplyAt,
                    s.RecieveAt,
                    s.DeliverAt,
                    s.Org,
                    s.ApplyStaff,
                    ApplicationDetails = s.ApplicationDetails.Select(ad => new
                    {
                        ad.ApplicationId,
                        ad.DetaillsId,
                        ad.ProductId,
                        ad.AppliedQty,
                        ad.DeliveredQty,
                        ad.ReceivedQty,
                        ad.Product,
                        ad.Product.ProdType,
                        ad.Product.ProdType.ProdCat,
                    })
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (result.Data == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Stock Application not found";
                return BadRequest(result);
            }
            return Ok(result);
        }

        // GET: api/StockApplication/getByIsDisputed/1
        [HttpGet("getByIsDisputed/{processStatus}")]
        public async Task<IActionResult> GetByIsDisputed(short IsDisputed)
        {
            var result = new Result<object>();
            try
            {
                result.Data = await _ablemusicContext.StockApplication
                .Include(s => s.ApplyStaff)
                .Include(s => s.Org)
                .Include(s => s.ApplicationDetails)
                .ThenInclude(s => s.Product)
                .ThenInclude(s => s.ProdType)
                .ThenInclude(s => s.ProdCat)
                .Where(s => s.IsDisputed == IsDisputed)
                .Select(s => new
                {
                    s.OrgId,
                    s.ApplyStaffId,
                    s.ApplyAt,
                    s.ApplicationId,
                    s.ProcessStatus,
                    s.ApplyReason,
                    s.ReplyContent,
                    s.IsDisputed,
                    s.ReplyAt,
                    s.RecieveAt,
                    s.DeliverAt,
                    s.Org,
                    s.ApplyStaff,
                    ApplicationDetails = s.ApplicationDetails.Select(ad => new
                    {
                        ad.ApplicationId,
                        ad.DetaillsId,
                        ad.ProductId,
                        ad.AppliedQty,
                        ad.DeliveredQty,
                        ad.ReceivedQty,
                        ad.Product,
                        ad.Product.ProdType,
                        ad.Product.ProdType.ProdCat,
                    })
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (result.Data == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Stock Application not found";
                return BadRequest(result);
            }
            return Ok(result);
        }

        // POST: api/StockApplication
        [CheckModelFilter]
        [HttpPost]
        public async Task<IActionResult> ApplyStock(StockApplicationViewModel stockApplicationViewModel)
        {
            var result = new Result<StockApplication>();
            var stockApplication = new StockApplication
            {
                OrgId = stockApplicationViewModel.OrgId,
                ApplyStaffId = stockApplicationViewModel.ApplyStaffId,
                ApplyAt = DateTime.UtcNow.ToNZTimezone(),
                ProcessStatus = 1,
                ApplyReason = stockApplicationViewModel.ApplyReason,
                ReplyContent = null,
                IsDisputed = 0,
                ReplyAt = null,
                RecieveAt = null,
                DeliverAt = null,
                ApplicationDetails = new List<ApplicationDetails>()
            };

            foreach (var pmq in stockApplicationViewModel.ProductIdMapQty)
            {
                var applicationDetails = new ApplicationDetails
                {
                    ProductId = pmq.Key,
                    AppliedQty = pmq.Value,
                    DeliveredQty = null,
                    ReceivedQty = null
                };
                stockApplication.ApplicationDetails.Add(applicationDetails);
            }

            try
            {   
                await _ablemusicContext.StockApplication.AddAsync(stockApplication);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = stockApplication;
            return Ok(result);
        }

        // PUT: api/StockApplication/reply/1/request approved
        [HttpPut("reply/{applicationId}/{replyContent}")]
        public async Task<IActionResult> ReplyStock(int applicationId, string replyContent)
        {
            var result = new Result<StockApplication>();
            var stockApplication = new StockApplication();
            try
            {
                stockApplication = await _ablemusicContext.StockApplication.Where(sa => sa.ApplicationId == applicationId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(stockApplication == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "stockApplication not found";
                return BadRequest(result);
            }

            stockApplication.ProcessStatus = 2;
            stockApplication.ReplyContent = replyContent;
            stockApplication.ReplyAt = DateTime.UtcNow.ToNZTimezone();

            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = stockApplication;
            return Ok(result);
        }

        // PUT: api/StockApplication/deliver
        [CheckModelFilter]
        [HttpPut("deliver")]
        public async Task<IActionResult> DeliverStock(StockApplicationDeliverAndReceiveViewModel stockApplicationDeliverViewModel)
        {
            var result = new Result<StockApplication>();
            var stockApplication = new StockApplication();
            try
            {
                stockApplication = await _ablemusicContext.StockApplication
                    .Include(sa => sa.ApplicationDetails)
                    .Where(sa => sa.ApplicationId == stockApplicationDeliverViewModel.ApplicationId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (stockApplication == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "stockApplication not found";
                return BadRequest(result);
            }

            stockApplication.ProcessStatus = 4;
            stockApplication.DeliverAt = DateTime.UtcNow.ToNZTimezone();
            foreach(var detail in stockApplication.ApplicationDetails)
            {
                foreach(var m in stockApplicationDeliverViewModel.ApplicationDetailsIdMapQty)
                {
                    if(detail.DetaillsId == m.Key)
                    {
                        detail.DeliveredQty = m.Value;
                    }
                }
            }

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

            result.Data = stockApplication;
            return Ok(result);
        }

        // PUT: api/StockApplication/receive
        [CheckModelFilter]
        [HttpPut("receive")]
        public async Task<IActionResult> ReceiveStock(StockApplicationDeliverAndReceiveViewModel stockApplicationReceiveViewModel)
        {
            var result = new Result<StockApplication>();
            var stockApplication = new StockApplication();
            try
            {
                stockApplication = await _ablemusicContext.StockApplication
                    .Include(sa => sa.ApplicationDetails)
                    .Where(sa => sa.ApplicationId == stockApplicationReceiveViewModel.ApplicationId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (stockApplication == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "stockApplication not found";
                return BadRequest(result);
            }

            stockApplication.ProcessStatus = 5;
            stockApplication.RecieveAt = DateTime.UtcNow.ToNZTimezone();
            foreach (var detail in stockApplication.ApplicationDetails)
            {
                foreach (var m in stockApplicationReceiveViewModel.ApplicationDetailsIdMapQty)
                {
                    if (detail.DetaillsId == m.Key)
                    {
                        detail.ReceivedQty = m.Value;
                        if(detail.ReceivedQty != detail.DeliveredQty)
                        {
                            stockApplication.IsDisputed = 1;
                        }
                    }
                }
            }

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

            result.Data = stockApplication;
            return Ok(result);
        }

        // PUT: api/StockApplication/solveDispute/1
        [CheckModelFilter]
        [HttpPut("solveDispute/{stockApplicationId}")]
        public async Task<IActionResult> SolveDisputeStock(int stockApplicationId)
        {
            var result = new Result<StockApplication>();
            var stockApplication = new StockApplication();
            try
            {
                stockApplication = await _ablemusicContext.StockApplication
                    .Include(sa => sa.ApplicationDetails)
                    .Where(sa => sa.ApplicationId == stockApplicationId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (stockApplication == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "stockApplication not found";
                return BadRequest(result);
            }

            stockApplication.IsDisputed = 0;

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

            result.Data = stockApplication;
            return Ok(result);
        }
    }
}