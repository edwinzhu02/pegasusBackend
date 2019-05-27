﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;


namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoListController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public TodoListController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        // GET: api/TodoList

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(short userId)
        {
            Result<List<TodoList>> result = new Result<List<TodoList>>();
            List<TodoList> todos = new List<TodoList>();
            try
            {
                todos = await _ablemusicContext.TodoList.Where(t => t.UserId == userId && t.TodoDate == DateTime.Now.Date).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if(todos.Count < 1)
            {
                result.IsSuccess = true;
                result.IsFound = false;
                result.ErrorMessage = "Todo not found";
                return Ok(result);
            }
            result.Data = todos;
            return Ok(result);
        }

        // PUT: api/TodoList/5
        [HttpPut("achieve/{id}")]
        public async Task<IActionResult> Put(short todoId)
        {
            Result<string> result = new Result<string>();
            TodoList todo;
            try
            {
                todo = await _ablemusicContext.TodoList.Where(t => t.ListId == todoId).FirstAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if(todo == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Todo not found";
                return BadRequest(result);
            }
            todo.ProcessedAt = DateTime.Now;
            todo.ProcessFlag = 1;
            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            result.IsSuccess = true;
            return Ok(result);
        }
    }
}
