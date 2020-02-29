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
    public class LessonMessageController : BasicController
    {
        private readonly IMapper _mapper;

        public LessonMessageController(ablemusicContext ablemusicContext, ILogger<ProductController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }


        //GET: http://localhost:5000/api/product/{id}
        [HttpGet]
        [Route("{id}/{roleType}/{lessonId}")]
        public async Task<ActionResult> GetMessages(int id,int roleType,int lessonId)
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.Product
                    .Where(x => x.ProductId == id)
                    .Include(x => x.ProdType.ProdCat)
                    .Select(x => new {
                        x.ProductId,
                        x.ProductName,
                        x.Model,
                        x.Brand,
                        x.SellPrice,
                        x.WholesalePrice,
                        ProdType = new
                        {
                            x.ProdType.ProdTypeId,
                            x.ProdType.ProdTypeName,
                            x.ProdType.ProdCatId,
                            PordCat = new { x.ProdType.ProdCat.ProdCatId, x.ProdType.ProdCat.ProdCatName }
                        }
                    })
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpPost]
        [CheckModelFilter]
        public async Task<ActionResult<Product>> PostMessage(ProductModel productModel)
        {
            //Result<List<Product>> result = new Result<List<Product>>();
            var result = new Result<object>();
            Product product = new Product();
            _mapper.Map(productModel, product);

            try
            {
                await _ablemusicContext.Product.AddAsync(product);
                await _ablemusicContext.SaveChangesAsync();

                //retrieve the new data with the ProductType and ProductCategory detail
                result.Data = await _ablemusicContext.Product
                    .Include(x => x.ProdType.ProdCat)
                    .Where(x => x.ProductId == (int)product.ProductId)
                    .Select(x => new {
                        x.ProductId,
                        x.ProductName,
                        x.Model,
                        x.Brand,
                        x.SellPrice,
                        x.WholesalePrice,
                        ProdType = new
                        {
                            x.ProdType.ProdTypeId,
                            x.ProdType.ProdTypeName,
                            x.ProdType.ProdCatId,
                            PordCat = new { x.ProdType.ProdCat.ProdCatId, x.ProdType.ProdCat.ProdCatName }
                        }
                    })
                    .ToListAsync();

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                _ablemusicContext.Remove(product);
                return BadRequest(result);
            }

            return Ok(result);

        }
    }
}