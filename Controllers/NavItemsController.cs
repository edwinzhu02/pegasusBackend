using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NavItemsController: BasicController
    {

        public NavItemsController(ablemusicContext ablemusicContext, ILogger<NavItemsController> log) : base(ablemusicContext, log)
        {
        }
        
        
        //GET: http://localhost:5000/api/navitems
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNavItems()
        {
            Result<Object> result = new Result<Object>();
            List<PageGroup> details = new List<PageGroup>();
            List<Page> detail;
            try
            {
                var userId = int.Parse(User.Claims.First(s => s.Type == "UserID").Value);
                var roleId = _ablemusicContext.User.FirstOrDefault(s => s.UserId == userId).RoleId;
                var pageList = _ablemusicContext.RoleAccess.Where(s => s.RoleId == roleId).Select(s => s.PageId).ToList();
                var pageGroups = _ablemusicContext.PageGroup.Include(s=>s.Page).ToList();
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