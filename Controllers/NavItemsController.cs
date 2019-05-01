using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            Result<IEnumerable<NavItem>> result = new Result<IEnumerable<NavItem>>();
            try
            {
                var userId = int.Parse(User.Claims.First(s => s.Type == "UserID").Value);
                var roleName = _pegasusContext.User.Include(s => s.Role).First(s => s.UserId == userId).Role
                    .RoleName;
                result.Data = GetNavItems(roleName);
                

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