using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockOrderController : BasicController
    {
        public StockOrderController(ablemusicContext ablemusicContext, ILogger<StockOrderController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/StockOrder
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new Result<Object>();
            try
            {
                result.Data = await _ablemusicContext.StockOrder.
                                Include(s=>s.Product).
                                Include(s=>s.Product).
                                ThenInclude(p=>p.ProdType).
                                Include(s=>s.Product).
                                ThenInclude(p=>p.ProdType).
                                ThenInclude(t=>t.ProdCat).
                                Include(s=>s.Org).
                                Include(s=>s.Staff).
                                Select(x=>new {x.OrderId,x.OrderType,x.ReceiptImg,x.Quantity,
                                x.BuyingPrice,x.OrgId,x.Org.OrgName,x.StaffId,x.Staff.FirstName,x.Staff.LastName,
                                x.Product.ProductId,x.Product.ProductName,x.Product.ProdType.ProdTypeId,x.Product.ProdType.ProdTypeName,
                                x.Product.ProdType.ProdCat.ProdCatId,x.Product.ProdType.ProdCat.ProdCatName,x.CreatedAt
                                }).                               
                                ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            // if(result.Data.Count <= 0)
            // {
            //     result.IsSuccess = false;
            //     result.ErrorMessage = "StockOrder not found";
            //     return BadRequest(result);
            // }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string productIdstr, [FromForm] string orgIdstr, [FromForm] string quantitystr,
            [FromForm] string pricestr, [FromForm] string staffIdstr, [FromForm(Name = "Receipt")] IFormFile ReceiptImg)
        {
            var result = new Result<StockOrder>();
            Product product = new Product();
            Stock stock;
            StockOrder stockOrder = new StockOrder();
            int productId;
            short orgId;
            int quantity;
            decimal price;
            short staffId;
            try
            {
                productId = Int32.Parse(productIdstr);
                orgId = Int16.Parse(orgIdstr);
                quantity = Int32.Parse(quantitystr);
                price = Convert.ToDecimal(pricestr);
                staffId = Int16.Parse(staffIdstr);
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            try
            {
                product = await _ablemusicContext.Product.Where(p => p.ProductId == productId).FirstOrDefaultAsync();
                stock = await _ablemusicContext.Stock.Where(s => s.OrgId == orgId && s.ProductId == productId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if (product == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Product not found";
                return BadRequest(result);
            }
            var uploadfileTime = toNZTimezone(DateTime.UtcNow);
            var uploadImageResult = UploadFile(ReceiptImg, "stock_order/receipt/", product.ProductId, uploadfileTime.ToString("yyMMddhhmmssfff"));
            if (!uploadImageResult.IsUploadSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = uploadImageResult.ErrorMessage;
                return BadRequest(result);
            }

            var imageAddress = "images/stock_order/receipt/" + product.ProductId.ToString() + uploadfileTime.ToString("yyMMddhhmmssfff") + 
                Path.GetExtension(ReceiptImg.FileName);

            if (stock == null)
            {
                stock = new Stock();
                stock.OrgId = orgId;
                stock.ProductId = productId;
                stock.Quantity = 0;
                try
                {
                    await _ablemusicContext.Stock.AddAsync(stock);
                    await _ablemusicContext.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    //delete file
                    try
                    {
                        System.IO.File.Delete(Path.Combine("wwwroot", imageAddress));
                    }catch(Exception e)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = ex.Message + '\n' + e.Message;
                        return BadRequest(result);
                    }
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
            }

            stock.Quantity += quantity;
            stockOrder.StockId = stock.StockId;
            stockOrder.OrderType = 2;
            stockOrder.ProductId = productId;
            stockOrder.OrgId = orgId;
            stockOrder.Quantity = quantity;
            stockOrder.BuyingPrice = price;
            stockOrder.ReceiptImg = imageAddress;
            stockOrder.StaffId = staffId;
            stockOrder.CreatedAt = toNZTimezone(DateTime.UtcNow);

            try
            {
                await _ablemusicContext.StockOrder.AddAsync(stockOrder);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //delete file
                try
                {
                    System.IO.File.Delete(Path.Combine("wwwroot", imageAddress));
                }
                catch (Exception e)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message + '\n' + e.Message;
                    return BadRequest(result);
                }
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = stockOrder;
            return Ok(result);
        }

    }
}
