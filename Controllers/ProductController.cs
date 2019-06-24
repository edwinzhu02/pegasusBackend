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
    public class ProductController : BasicController
    {
        private readonly IMapper _mapper;

        public ProductController(ablemusicContext ablemusicContext, ILogger<ProductController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }


        //GET: http://localhost:5000/api/product
        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.Product
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

        //GET: http://localhost:5000/api/product/{id}
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetProduct(int id)
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

        [Route("[action]/{typeid}")]
        [HttpGet]
        public async Task<ActionResult> GetProdByType(int typeid)
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.Product
                    .Where(x => x.ProdTypeId == typeid)
                    .Include(x => x.ProdType)
                    .ThenInclude(cate=>cate.ProdCat)
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

        [Route("[action]/{cateid}")]
        [HttpGet]
        public async Task<ActionResult> GetTypeByCat(int cateid)
        {
            Result<Object> result = new Result<object>();
            try
            {

                result.IsSuccess = true;

                result.Data = await _ablemusicContext.ProdCat
                    .Where(x => x.ProdCatId == cateid)
                    .Include(x => x.ProdType)
                    .Select(x => new {
                        x.ProdCatId, 
                        x.ProdCatName, 
                        ProdType = x.ProdType
                            .Select(s => new {
                                s.ProdTypeId, 
                                s.ProdTypeName 
                            })
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

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetCat()
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext
                    .ProdCat
                    .Select(x => new {
                        x.ProdCatId, 
                        x.ProdCatName
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

        //PUT: api/Product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductModel productModel)
        {
            var result = new Result<object>();
            Type productType = typeof(Product);
            Product product = new Product();
            _mapper.Map(productModel, product);
            var updateProd = await _ablemusicContext.Product
                .Where(x => x.ProductId == id)
                .FirstOrDefaultAsync();
            product.ProductId = id;
            if (updateProd == null)
            {
                return NotFound(DataNotFound(result));
            }
            UpdateTable(product, productType, updateProd);
            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            result.Data = product;
            return Ok(result);

        }



        [HttpPost]
        [CheckModelFilter]
        public async Task<ActionResult<Product>> PostProduct(ProductModel productModel)
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