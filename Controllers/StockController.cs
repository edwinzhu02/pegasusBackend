using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : BasicController
    {
        public StockController(ablemusicContext ablemusicContext, ILogger<StockController> log) : base(ablemusicContext, log)
        {
        }

       [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                var stock = await _ablemusicContext.Stock.FirstOrDefaultAsync(s => s.StockId == id);
                if (stock == null)
                {
                    throw new Exception("Learner does not exist");
                }
                _ablemusicContext.Remove(stock);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = "delete successfully";
            return Ok(result);
        }

        // GET: api/Stock
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Result<List<Object>> result = new Result<List<Object>>();
            dynamic stocks = new List<Object>();
            try
            {
                stocks = await (from s in _ablemusicContext.Stock
                                join p in _ablemusicContext.Product on s.ProductId equals p.ProductId
                                join pt in _ablemusicContext.ProdType on p.ProdTypeId equals pt.ProdTypeId
                                join pc in _ablemusicContext.ProdCat on pt.ProdCatId equals pc.ProdCatId
                                join o in _ablemusicContext.Org on s.OrgId equals o.OrgId
                                select new 
                                {
                                    StockId = s.StockId,
                                    Quantity = s.Quantity,
                                    Org = new 
                                    {
                                        OrgId = o.OrgId,
                                        OrgName = o.OrgName,
                                    },
                                    Product = new 
                                    {
                                        ProductId = p.ProductId,
                                        ProductName = p.ProductName,
                                        Model = p.Model,
                                        Brand = p.Brand,
                                        SellPrice = p.SellPrice,
                                        WholesalePrice = p.WholesalePrice,
                                        ProdType = new 
                                        {
                                            ProdTypeId = pt.ProdTypeId,
                                            ProdTypeName = pt.ProdTypeName,
                                            ProdCat = new 
                                            {
                                                ProdCatId = pc.ProdCatId,
                                                ProdCatName = pc.ProdCatName,
                                            }
                                        }
                                    }
                                }).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(stocks.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "No any stocks found";
                return BadRequest(result);
            }
            result.Data = new List<Object>();
            foreach(var s in stocks)
            {
                result.Data.Add(s);
            }
            return Ok(result);
        }        
    }
}
