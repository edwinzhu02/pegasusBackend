using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController: BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public PayrollController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        [HttpGet("{date}")]
        public async Task<IActionResult> GetPayrollByMonth(DateTime date)
        {
            var result = new Result<Object>();
            try
            {
                var teacherTransaction = _ablemusicContext.TeacherTransaction
                    .Where(s => s.CreatedAt.Value.Month == date.Month)
                    .GroupBy(s=>s.TeacherId)
                    .Select(s=>new{MonthWage=s.Sum(q=>q.WageAmount)})
                    .ToList();
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