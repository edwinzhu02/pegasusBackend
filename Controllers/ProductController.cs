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

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController: BasicController
    {
        
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly IMapper _mapper;

        public ProductController(pegasusContext.pegasusContext pegasusContext, IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }


        //GET: http://localhost:5000/api/product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            Result<List<Product>> result = new Result<List<Product>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.Product.Include(x => x.ProdType.ProdCat).ToListAsync();
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
        public ActionResult<List<Product>>GetProduct(int id)
        {
            Result<List<Product>> result = new Result<List<Product>>();
            try
            {
                result.IsSuccess = true;
                result.Data = _pegasusContext.Product.Where(x => x.ProductId == id).Include(x => x.ProdType.ProdCat).ToList();
                return Ok(result);
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [Route("[action]/{typeid}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProdByType(int typeid)
        {
            Result<List<Product>> result = new Result<List<Product>>();
            try 
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.Product
                .Where(x => x.ProdTypeId == typeid)
                .Include(x=>x.ProdType.ProdCat)
                //.Select(x => x)
                .ToListAsync();

                return Ok(result);
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

        }

        [Route("[action]/{cateid}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdType>>> GetTypeByCat(int cateid)
        {
            Result<List<ProdType>> result = new Result<List<ProdType>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.ProdType
                .Where(x => x.ProdCatId == cateid)
                .Include(x => x.ProdCat)
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
        public async Task<ActionResult<IEnumerable<ProdCat>>> GetCat()
        {
            Result<List<ProdCat>> result = new Result<List<ProdCat>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.ProdCat.ToListAsync();

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
            var result = new Result<string>();
            Type prodType = typeof(Product);
            var updateProd = await _pegasusContext.Product.Where(x => x.ProductId == id).FirstOrDefaultAsync();
            if (updateProd == null)
            {
                return NotFound(DataNotFound(result));
            }
            UpdateTable(productModel, prodType, updateProd);
            try
            {
                await _pegasusContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductModel productModel)
        {
            Result<List<Product>> result = new Result<List<Product>>();
            Product product = new Product();
            _mapper.Map(productModel, product);

            try
            {
                await _pegasusContext.Product.AddAsync(product);
                await _pegasusContext.SaveChangesAsync();

                //retrieve the new data with the ProductType and ProductCategory detail
                result.Data= await _pegasusContext.Product
                    .Include(x => x.ProdType.ProdCat)
                    .Where(x=>x.ProductId==(int)product.ProductId)
                    .ToListAsync();

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                _pegasusContext.Remove(product);
                return BadRequest(result);
            }

            return Ok(result);

        }
    }
}