using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController: BasicController
    {
        private readonly IMapper _mapper;

        public StaffController(ablemusicContext ablemusicContext, ILogger<StaffController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffs()
        {
            var result = new Result<Object>();
            try
            {
                result.Data = await _ablemusicContext.Staff
                    .Include(s=>s.User)
                    .ThenInclude(s=>s.Role)
                    .Include(s=>s.StaffOrg)
                    .Where(s=>s.IsActivate==1)
                    .Select(s=>new
                    {
                        s.Dob,s.Visa,s.Email,s.Photo,s.Gender,s.IdType,
                        s.IdPhoto,s.StaffId,s.IdNumber,s.LastName,s.FirstName,s.HomePhone,s.IrdNumber,
                        s.StaffOrg,s.ExpiryDate,s.StockOrder,s.MobilePhone,s.User.Role.RoleName,s.User.Role.RoleId
                    })
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(short id)
        {
            var result = new Result<string>();
            try
            {
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.StaffId == id);
                if (staff == null)
                {
                    return NotFound(DataNotFound(result));
                }

                staff.IsActivate = 0;
                _ablemusicContext.Update(staff);
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

        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromForm(Name = "IdPhoto")] IFormFile idPhoto,
            [FromForm(Name = "Photo")] IFormFile photo,[FromForm] string details)
        {
            var result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var detailsJson = JsonConvert.DeserializeObject<StaffModel>(details);
                    if (await _ablemusicContext.Staff.FirstOrDefaultAsync(s => s.IdNumber == detailsJson.IdNumber) !=
                        null)
                    {
                        throw new Exception("Staff has exist");
                    }
                    var newStaff = new Staff();
                    _mapper.Map(detailsJson, newStaff);
                    newStaff.IsActivate = 1;
                    _ablemusicContext.Add(newStaff);
                    await _ablemusicContext.SaveChangesAsync();

                    //create user for this staff
                    var newUser = new User
                    {
                        UserName = newStaff.Email,Password = "helloworld",
                        CreatedAt = toNZTimezone(DateTime.UtcNow),IsActivate = 1, RoleId = detailsJson.RoleId
                    };
                    _ablemusicContext.Add(newUser);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //keep the userId to staff table
                    newStaff.UserId = newUser.UserId;
                    _ablemusicContext.Update(newStaff);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //upload filesΩ
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");
                    if (idPhoto != null)
                    {
                        newStaff.IdPhoto = $"images/staff/IdPhotos/{newStaff.StaffId+strDateTime+Path.GetExtension(idPhoto.FileName)}";
                        _ablemusicContext.Update(newStaff);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(idPhoto,"staff/IdPhotos/",newStaff.StaffId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (photo != null)
                    {
                        newStaff.Photo = $"images/staff/Photos/{newStaff.StaffId+strDateTime+Path.GetExtension(photo.FileName)}";
                        _ablemusicContext.Update(newStaff);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(photo,"staff/Photos/",newStaff.StaffId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }
                    
                    dbContextTransaction.Commit();

                }
                result.Data = "success!";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff([FromForm(Name = "IdPhoto")] IFormFile idPhoto,
            [FromForm(Name = "Photo")] IFormFile photo,[FromForm] string details, short id)
        {
            var result = new Result<string>();
            try
            {
                using (var dbContextTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    var staff = await _ablemusicContext.Staff.FirstOrDefaultAsync(s => s.StaffId == id);
                    if (staff == null)
                    {
                        return NotFound(DataNotFound(result));
                    }
                    var detailsJson = JsonConvert.DeserializeObject<StaffModel>(details);
                
                    //delete exist staff orgs
                    _ablemusicContext.StaffOrg.Where(s=>s.StaffId==staff.StaffId).ToList().ForEach(s => { _ablemusicContext.Remove(s);});
                    await _ablemusicContext.SaveChangesAsync();

                    //update basic staff info
                    _mapper.Map(detailsJson, staff);
                    _ablemusicContext.Update(staff);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //update role id
                    var user = _ablemusicContext.User.FirstOrDefault(s => s.UserId == staff.UserId);
                    user.RoleId = detailsJson.RoleId;
                    _ablemusicContext.Update(user);
                    await _ablemusicContext.SaveChangesAsync();
                
                    //upload files
                    var strDateTime = toNZTimezone(DateTime.UtcNow).ToString("yyMMddhhmmssfff");
                    if (idPhoto != null)
                    {
                        staff.IdPhoto = $"images/staff/IdPhotos/{staff.StaffId+strDateTime+Path.GetExtension(idPhoto.FileName)}";
                        _ablemusicContext.Update(staff);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(idPhoto,"staff/IdPhotos/",staff.StaffId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }

                    if (photo != null)
                    {
                        staff.Photo = $"images/staff/Photos/{staff.StaffId+strDateTime+Path.GetExtension(photo.FileName)}";
                        _ablemusicContext.Update(staff);
                        await _ablemusicContext.SaveChangesAsync();
                        var uploadResult = UploadFile(photo,"staff/Photos/",staff.StaffId,strDateTime);
                        if (!uploadResult.IsUploadSuccess)
                        {
                            throw new Exception(uploadResult.ErrorMessage);
                        }
                    }
                    
                    dbContextTransaction.Commit();
                }


            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.Data = "success!";
            return Ok(result);
        }
    }
}