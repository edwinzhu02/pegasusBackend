using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Crmf;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers.Authorization
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController: BasicController
    {

        public ChangePasswordController(ablemusicContext ablemusicContext, ILogger<ChangePasswordController> log) : base(ablemusicContext, log)
        {
        }

        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var result = new Result<string>();
            try
            {
                var user = _ablemusicContext.User.FirstOrDefault(s => s.UserName == model.userName);
                if (user == null)
                {
                    throw new Exception("user does not exist");
                }

                if (user.Password != model.oldPassword)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "The old password is incorrect";
                    return StatusCode(401, result);
                }

                if (user.Password == model.newPassword)
                {
                    throw new Exception("The new password is same as the old password");
                }

                user.Password = model.newPassword;
                _ablemusicContext.Update(user);
                await _ablemusicContext.SaveChangesAsync();
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