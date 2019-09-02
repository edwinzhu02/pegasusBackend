using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.IO;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
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
    public class LoginController: BasicController
    {
        private readonly ApplicationSettings _appSettings;
        public LoginController(ablemusicContext ablemusicContext, ILogger<LoginController> log, IOptions<ApplicationSettings> appSettings) : base(ablemusicContext, log)
        {
            _appSettings = appSettings.Value;
        }
       //POST: http://localhost:5000/api/login
        [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> GetProfileImg(int userId)
        {
            Result<string> result = new Result<string>();
            try
            {
                var user = await _ablemusicContext.User.Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Staff)
                    .Include(s=>s.Role)
                    .Where(s =>s.UserId ==userId)
                    .FirstOrDefaultAsync();
                string photo;
                if (user.Role.RoleId  ==1)  //teacher 
                    photo =  user.Teacher.FirstOrDefault().Photo;
                else if (user.Role.RoleId  ==4)  //learner
                    photo =  user.Learner.FirstOrDefault().Photo;
                else   //staff
                {
                    photo =  user.Staff.FirstOrDefault().Photo;
                }
                result.Data = photo;
            }  
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return StatusCode(401, result);
            }
            return Ok(result);
        }        
       [HttpPost("[action]/{userId}")]
        public async Task<IActionResult> ChangeImg(int userId, [FromForm(Name = "Photo")] IFormFile Photo)
        {
            Result<string> result = new Result<string>();
            var uploadResult= new UploadFileModel();
            var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff"); 
            try
            {
                var user = await _ablemusicContext.User.Include(s => s.Learner)
                    .Include(s => s.Teacher)
                    .Include(s => s.Staff)
                    .Include(s => s.Role)
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();
                if (user.Role.RoleId == 1){ //teacher {
                    var teacher = user.Teacher.FirstOrDefault();
                    teacher.Photo = $"images/tutor/Photos/{teacher.TeacherId + strDateTime + Path.GetExtension(Photo.FileName)}";
                    _ablemusicContext.Update(teacher);
                    await _ablemusicContext.SaveChangesAsync();
                    uploadResult = UploadFile(Photo, "tutor/Photos/", teacher.TeacherId, strDateTime);
                }
                else if (user.Role.RoleId == 4)  {//learner
                    var learner = user.Learner.FirstOrDefault();
                    
                    learner.Photo = $"images/learner/Photos/{learner.LearnerId + strDateTime + Path.GetExtension(Photo.FileName)}";
                    _ablemusicContext.Update(learner);
                    await _ablemusicContext.SaveChangesAsync();
                    uploadResult = UploadFile(Photo, "learner/Photos/", learner.LearnerId, strDateTime);                   
                }
                else   //staff
                {
                    var staff = user.Staff.FirstOrDefault();
                    staff.Photo = $"images/staff/Photos/{staff.StaffId + strDateTime + Path.GetExtension(Photo.FileName)}";
                    _ablemusicContext.Update(staff);
                    await _ablemusicContext.SaveChangesAsync();
                    uploadResult = UploadFile(Photo, "staff/Photos/", staff.StaffId, strDateTime);                   

                }
       

                if (!uploadResult.IsUploadSuccess)
                {
                    throw new Exception(uploadResult.ErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        }        
      [HttpPost("[action]/{userId}")]
        public async Task<IActionResult> ChangeProfile(int userId, [FromBody] ProfileModel profile)
        {
            Result<string> result = new Result<string>();
            try
            {
                var user = await _ablemusicContext.User.Where(s => s.UserId == userId)
                    .Include(s => s.Learner)
                    .Include(s => s.Learner).ThenInclude(l => l.Parent)                    
                    .Include(s => s.Teacher)
                    .Include(s => s.Staff)
                    .Include(s => s.Role)
                    .FirstOrDefaultAsync();
                if (user == null) throw new Exception("User does not exist");
                if (user.Role.RoleId == 1){ //teacher {
                    var teacher = user.Teacher.FirstOrDefault();
                    teacher.MobilePhone = profile.ContactNum;
                    teacher.Email = profile.Email;
                    _ablemusicContext.Update(teacher);
                    await _ablemusicContext.SaveChangesAsync();                  
                }
                else if (user.Role.RoleId == 4)  {//learner
                    var learner = user.Learner.FirstOrDefault();
                    learner.ContactNum = profile.ContactNum;
                    learner.Email = profile.Email;   
                    _ablemusicContext.Update(learner);
                    await _ablemusicContext.SaveChangesAsync();  
                    var parent = user.Learner.FirstOrDefault().Parent.FirstOrDefault();
                    parent.ContactNum = profile.ContactNum;
                    parent.Email = profile.Email;   
                    _ablemusicContext.Update(parent);
                    await _ablemusicContext.SaveChangesAsync();                                                                       
                }
                else   //staff
                {
                    var staff = user.Staff.FirstOrDefault();
                    staff.MobilePhone = profile.ContactNum;
                    staff.Email = profile.Email;  
                    _ablemusicContext.Update(staff);
                    await _ablemusicContext.SaveChangesAsync();                     
                }
                return Ok(result);
            }
            catch(Exception ex){
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            
        }        
      [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            Result<object> result = new Result<object>();
            try
            {
                var user = await _ablemusicContext.User.Where(s => s.UserId == userId)
                    .Include(s => s.Learner)
                    .Include(s => s.Learner).ThenInclude(l => l.Parent)                    
                    .Include(s => s.Teacher)
                    .Include(s => s.Staff)
                    .Include(s => s.Role)                    
                    .FirstOrDefaultAsync();
                if (user == null) throw new Exception("User does not exist");
                if (user.Role.RoleId == 1){ //teacher {
                    var teacher = user.Teacher.FirstOrDefault();
                    result.Data =new  { FirstName = teacher.FirstName,
                        LastName = teacher.LastName,
                        ContactNum = teacher.MobilePhone,
                        Email = teacher.Email};
                }
                else if (user.Role.RoleId == 4)  {//learner
                    var learner = user.Learner.FirstOrDefault();
                    result.Data =new  { FirstName = learner.FirstName,
                        LastName = learner.LastName,
                        ContactNum = learner.ContactNum,
                        Email = learner.Email};                
                }
                else   //staff
                {
                    var staff = user.Staff.FirstOrDefault();
                    result.Data =new  { FirstName = staff.FirstName,
                        LastName = staff.LastName,
                        ContactNum = staff.MobilePhone,
                        Email = staff.Email};
                }
                return Ok(result);
            }
            catch(Exception ex){
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            
        }                
        //POST: http://localhost:5000/api/login
        [CheckModelFilter]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UsrAndPass model)
        {
            Result<Object> result = new Result<object>();
           
            try
            {
                var user = await _ablemusicContext.User.Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Staff)
                    .ThenInclude(w=>w.StaffOrg)
                    .ThenInclude(q=>q.Org)
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
                var position = user.Role.RoleName;
                var date = DateTimeOffset.UtcNow.AddDays(14).ToUnixTimeSeconds();
                string photo;
                if (user.Role.RoleId  ==1)  //teacher 
                    photo =  user.Teacher.FirstOrDefault().Photo;
                else if (user.Role.RoleId  ==4)  //learner
                    photo =  user.Learner.FirstOrDefault().Photo;
                else   //staff
                {
                    photo =  user.Staff.FirstOrDefault().Photo;
                }

                
                result.Data = new {
                    token=tokenToClient,username=model.UserName,roleid = user.RoleId,userid=user.UserId,
                    expires=date,userdetails= UserInfoFilter(user,position),photo=photo,
                    mobileComponents = ComponentsSelectors(user.RoleId)
                            
                };
                result.IsSuccess = true;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return StatusCode(401, result);
            }
            
            
        }

        private List<object> ComponentsSelectors(short? roleId)
        {
            switch (roleId)
            {
                case 1:
                    return new List<object>
                    {
                        new { name= "Check In", code= "#1abc9c",component= "CheckIn" },
                        new { name= "Check Out", code= "#2ecc71",component= "CheckOut" },
                        new { name= "Schedule", code= "#3498db",component= "Schedule" },
                        new { name= "Feedback", code= "#9b59b6",component= "Feedback" },
                        new { name= "View Feedback", code= "#27ae60",component= "FeedbackView" },
                    };
                case 2:
                    return new List<object>();
                case 3:
                    return new List<object>
                    {
                        new { name= "Check In", code= "#1abc9c",component= "CheckIn" },
                        new { name= "Check Out", code= "#2ecc71",component= "CheckOut" },
                    };
                case 4:
                    return new List<object>
                    {
                        new { name= "Feedback", code= "#9b59b6",component= "Feedback" },
                        new { name= "View Feedback", code= "#27ae60",component= "FeedbackView" }
                    };
                case 5:
                    return new List<object>();
                default:
                    return new List<object>();
            }
        }
        
    }
}