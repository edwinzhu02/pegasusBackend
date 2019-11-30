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
    public class TestTodoListController : BasicController
    {
        private readonly IMapper _mapper;

        public TestTodoListController(ablemusicContext ablemusicContext, ILogger<TestTodoListController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }


        //GET: http://localhost:5000/api/product
        [HttpGet]
        public async Task<ActionResult> GetTestTodoList()
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.TestTodoList
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
        public async Task<ActionResult> GetTestTodo(int id)
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.TestTodoList
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
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
        [HttpPut()]
        public async Task<IActionResult> PutProduct( TestTodoList testTodoList)
        {
            var result = new Result<object>();
            Type modelType = typeof(TestTodoList);
            // TestTodoList findTestTodoList = new TestTodoList();
            // _mapper.Map(testTodoList, findTestTodoList);
            var updateTestTodoList = await _ablemusicContext.TestTodoList
                .Where(x => x.Id == testTodoList.Id)
                .FirstOrDefaultAsync();
            if (updateTestTodoList == null)
            {
                return NotFound(DataNotFound(result));
            }
            UpdateTable(testTodoList, modelType, updateTestTodoList);
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
            result.Data = updateTestTodoList;
            return Ok(result);
        }



        [HttpPost]
        [CheckModelFilter]
        public async Task<ActionResult<TestTodoList>> PostProduct(TestTodoList testTodoList)
        {
            //Result<List<Product>> result = new Result<List<Product>>();
            var result = new Result<object>();
            try
            {
                await _ablemusicContext.TestTodoList.AddAsync(testTodoList);
                await _ablemusicContext.SaveChangesAsync();

                //retrieve the new data with the ProductType and ProductCategory detail
                result.Data = testTodoList;

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                // _ablemusicContext.Remove(product);
                return BadRequest(result);
            }

            return Ok(result);

        }
    }
}