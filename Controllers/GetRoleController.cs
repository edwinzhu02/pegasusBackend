using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetRoleController: ControllerBase
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;

        public GetRoleController(pegasusContext.pegasusContext pegasusContext)
        {
            _pegasusContext = pegasusContext;
        }
        
        //Post: http://localhost:5000/api/getrole
        /*{
            "username": "",
            "password":""
        }*/
        [HttpPost]
        public Result<User> getRole([FromBody] UsrAndPass UserAndPass)
        {
            Result<User> result = new Result<User>();
            var User = _pegasusContext.User.FirstOrDefault(s => s.UserName == UserAndPass.username);
            if (User == null)
            {
                result.IsSuccess = false;
                result.ErrorCode = "401";
                result.ErrorMessage = "Username does not exist.";
                result.Data = new User();
                return result;
            }else if (User.Password != UserAndPass.password)
            {
                result.IsSuccess = false;
                result.ErrorCode = "401";
                result.ErrorMessage = "The password is incorrect";
                result.Data = new User();
                return result;
            }
            else
            {
                
                var details = _pegasusContext.User
                    .Include(s => s.LoginLog)
                    .Include(w => w.Role)
                    .ThenInclude(c => c.RoleAccess)
                    .ThenInclude(q => q.Page)
                    .ThenInclude(r => r.PageGroup)
                    .Include(a => a.OnlineUser);

                result.Data = details.FirstOrDefault(s => s.UserName == UserAndPass.username);

                return result;
            }
        }

    }
}