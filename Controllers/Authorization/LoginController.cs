using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Crmf;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Controllers.Authorization
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController: BasicController
    {
        private readonly pegasusContext.pegasusContext _pegasusContext;
        private readonly ApplicationSettings _appSettings;
        public LoginController(pegasusContext.pegasusContext pegasusContext, IOptions<ApplicationSettings> appSettings)
        {
            _pegasusContext = pegasusContext;
            _appSettings = appSettings.Value;
        }
        
        /*[Authorize]
        [HttpGet]
        public ActionResult get()
        {
            var a = User.Claims.First(s=>s.Type=="UserID").Value;
            return Ok(a);
        }*/
        
        
        //POST: http://localhost:5000/api/login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UsrAndPass model)
        {
            Result<Object> result = new Result<object>();
           
            try
            {
                var user = await _pegasusContext.User.Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Staff)
                    .Include(s=>s.Role)
                    .FirstOrDefaultAsync(s=>s.UserName==model.UserName);
                
                //Username is not be registered
                if (user == null)
                {
                    result.IsSuccess = false;
                    throw new Exception("The user does not exist.");
                }
                //Case when password is not correct
                if (user.Password != model.Password)
                {
                    result.IsSuccess = false;
                    throw new Exception("The password is incorrect.");
                }
                //token Details
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID", user.UserId.ToString()),
                    }),
                    Expires = DateTime.UtcNow.AddDays(14),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)),
                        SecurityAlgorithms.HmacSha256Signature)

                };

                var tokenHandle = new JwtSecurityTokenHandler();
                var securityToken = tokenHandle.CreateToken(tokenDescriptor);
                var tokenToClient = tokenHandle.WriteToken(securityToken);
                result.Data = new {token=tokenToClient,roleid = user.RoleId,userid=user.UserId,role=user.Role.RoleName,expires=DateTime.UtcNow.AddDays(14),userdetails= UserInfoFilter(user)};
                result.IsSuccess = true;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return StatusCode(401, result);
            }
            
            
        }
    }
}