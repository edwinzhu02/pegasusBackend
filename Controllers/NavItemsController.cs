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
            Result<Object> result = new Result<Object>();
            PageGroup details;
            try
            {
                var userId = int.Parse(User.Claims.First(s => s.Type == "UserID").Value);
                var roleId = _pegasusContext.User.FirstOrDefault(s => s.UserId == userId).RoleId;
                var pageList = _pegasusContext.RoleAccess.Where(s => s.RoleId == roleId).Select(s => s.PageId).ToList();
                pageList.ForEach(s =>
                {
                    _pegasusContext.PageGroup.ToList().ForEach(w =>
                    {
                        
                    });
                });
                result.Data = "";

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