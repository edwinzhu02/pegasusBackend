﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using System.IO;


namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockOrderController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public StockOrderController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        // GET: api/StockOrder
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new Result<List<StockOrder>>();
            try
            {
                result.Data = await _ablemusicContext.StockOrder.ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(result.Data.Count <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "StockOrder not found";
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string productIdstr, [FromForm] string orgIdstr, [FromForm] string quantitystr,
            [FromForm] string pricestr, [FromForm] string staffIdstr, [FromForm(Name = "Receipt")] IFormFile ReceiptImg)
        {
            var result = new Result<StockOrder>();
            Product product = new Product();
            Stock stock = new Stock();
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
            if (productId <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "productId is required";
                return BadRequest(result);
            }
            if (orgId <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "orgId is required";
                return BadRequest(result);
            }
            if (quantity <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "quantity is required";
                return BadRequest(result);
            }
            if (price <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "price is required";
                return BadRequest(result);
            }
            if (staffId <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "staffId is required";
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
            var uploadfileTime = DateTime.Now;
            var uploadImageResult = UploadFile(ReceiptImg, "stock_order/receipt/", product.ProductId, uploadfileTime.ToString());
            if (!uploadImageResult.IsUploadSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = uploadImageResult.ErrorMessage;
                return BadRequest(result);
            }

            var imageAddress = "images/stock_order/receipt/" + product.ProductId.ToString() + uploadfileTime.ToString() + Path.GetExtension(ReceiptImg.FileName);

            if (stock == null)
            {

            }
            else
            {
                stockOrder.StockId = stock.StockId;
                stockOrder.OrderType = 2;
                stockOrder.ProductId = productId;
                stockOrder.OrgId = orgId;
                stockOrder.Quantity = quantity;
                stockOrder.BuyingPrice = price;
                stockOrder.ReceiptImg = imageAddress;
                stockOrder.StaffId = staffId;
                stockOrder.CreatedAt = DateTime.Now;
            }

            try
            {
                await _ablemusicContext.StockOrder.AddAsync(stockOrder);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //delete file
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = stockOrder;
            return Ok(result);
        }

    }
}
