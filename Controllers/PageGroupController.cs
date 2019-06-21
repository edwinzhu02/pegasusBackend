﻿using System;
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

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageGroupController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;

        public PageGroupController(pegasusContext.ablemusicContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }

        // GET: api/PageGroup
        [HttpGet]
        public async Task<IActionResult> GetPageGroup()
        {
            var result = new Result<Object>();
            try
            {
                result.Data = await _pegasusContext.PageGroup.ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }



        // POST: api/PageGroup
        [HttpPost]
        [CheckModelFilter]
        public async Task<IActionResult> PostPageGroup(PageGroup pagegroup)
        {
            var result = new Result<object>();
            PageGroup pageGroup = new PageGroup();
            try
            {
                await _pegasusContext.PageGroup.AddAsync(pagegroup);
                await _pegasusContext.SaveChangesAsync();
                result.Data = "success";
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            return Ok(result);
        }

        // PUT: api/PageGroup/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPageGroup(int id, PageGroup pagegroup)
        {
            var result = new Result<object>();

            try
            {
                var pageGroup = await _pegasusContext.PageGroup
           .Where(x => x.PageGroupId == id)
           .FirstOrDefaultAsync();
                if (pageGroup == null)
                {
                    return NotFound(DataNotFound(result));
                }
                pageGroup = await _pegasusContext.PageGroup
                .Where(s => s.PageGroupId == id).FirstOrDefaultAsync();
                pageGroup.PageGroupName = pagegroup.PageGroupName;
                pageGroup.DisplayOrder = pagegroup.DisplayOrder;
                pageGroup.Icon = pagegroup.Icon;
                _pegasusContext.Update(pageGroup);
                await _pegasusContext.SaveChangesAsync();
                result.Data = "success";
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            result.Data = pagegroup;
            return Ok(result);


        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePageGroup(int id)
        {
            var result = new Result<object>();

            try
            {
                var pageGroup = await _pegasusContext.PageGroup
                        .Where(s => s.PageGroupId == id).FirstOrDefaultAsync();
                if (pageGroup == null)
                {
                    return NotFound(DataNotFound(result));
                }
                _pegasusContext.Remove(pageGroup);
                await _pegasusContext.SaveChangesAsync();
                result.Data = "success";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
