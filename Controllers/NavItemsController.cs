using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NavItemsController: BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public NavItemsController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult GetNavItems()
        {
            Result<Object> result = new Result<Object>();
            List<PageGroup> details = new List<PageGroup>();
            List<Page> detail;
            try
            {
                var userId = int.Parse(User.Claims.First(s => s.Type == "UserID").Value);
                var roleId = _pegasusContext.User.FirstOrDefault(s => s.UserId == userId).RoleId;
                var pageList = _pegasusContext.RoleAccess.Where(s => s.RoleId == roleId).Select(s => s.PageId).ToList();
                var pageGroups = _pegasusContext.PageGroup.Include(s=>s.Page).ToList();
                pageGroups.ForEach(pageGroup =>
                {
                    detail = new List<Page>();
                    pageGroup.Page.ToList().ForEach(page =>
                    {
                        if (pageList.Contains(page.PageId))
                        {
                            
                            detail.Add(page);
                        }
                    });

                    pageGroup.Page = detail;
                    
                    details.Add(pageGroup);
                    
                });
                var finaldata = details.Where(s => s.Page.Count != 0).ToList();
                result.Data = finaldata;

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