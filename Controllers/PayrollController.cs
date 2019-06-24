using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController: BasicController
    {

        public PayrollController(ablemusicContext ablemusicContext, ILogger<PayrollController> log) : base(ablemusicContext, log)
        {
        }


        [HttpGet("{begindate}/{enddate}")]
        public async Task<IActionResult> GetPayrollBetweenDate(DateTime begindate,DateTime enddate)
        {
            var result = new Result<Object>();
            try
            {
                var teacherSalary = await _ablemusicContext.TeacherTransaction
                    .Where(s=>begindate<=s.CreatedAt.Value.Date && s.CreatedAt.Value.Date<=enddate)
                    .GroupBy(g => g.TeacherId)
                    .Select(s => new {
                        s.First().TeacherId, TeacherFirstName=s.First().Teacher.FirstName,
                        TeacherLastName=s.First().Teacher.LastName,Wage = s.Sum(c => c.WageAmount)
                    })
                    .ToListAsync();
                result.Data = new{begindate=begindate,enddate=enddate,TeacherSalary=teacherSalary};
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