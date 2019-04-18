using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using AutoMapper;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController: ControllerBase
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
            return await _pegasusContext.Product.Include(x => x.ProdType.ProdCat).ToListAsync();
        }

        //GET: http://localhost:5000/api/product/{id}
        [HttpGet]
        [Route("{id}")]
        public ActionResult<List<Product>>GetProduct(int id)
        {
            return _pegasusContext.Product.Where(x => x.ProductId == id).Include(x=>x.ProdType.ProdCat).ToList();
        }
        // public ActionResult<List<CourseCategory>> GetProduct(int id)
        // {
        //     return _pegasusContext.CourseCategory.Where(s => s.CourseCategoryName.ToLower().Contains(name.ToLower())).ToList();
        // }




    }
}