using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class InvoiceController : BasicController
    {

        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private readonly IMapper _mapper;

        public InvoiceController(pegasusContext.ablemusicContext pegasusContext, IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }
        
        //GET: http://localhost:5000/api/invoice/:studentId
        [HttpGet]
        [Route("{id}")]
        public ActionResult<List<Invoice>> GetInvoice(int id)
        {
            Result<string> result = new Result<string>();
            try
            {
                return _pegasusContext.Invoice.Where(s => s.LearnerId == id&&s.IsActive==1).Include(s => s.Term)
                    .Where(s=>s.IsPaid == 0)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        
        // GET: http://localhost:5000/api/invoice
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            //_pegasusContext.Detach(Invoice);
            Result<List<Invoice>> result = new Result<List<Invoice>>();


            try
            {
                result.Data = await _pegasusContext.Invoice.Include(x => x.Learner).Include(x => x.Term).Include(x => x.GroupCourseInstance).Include(x => x.CourseInstance).ToListAsync();
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            result.IsSuccess = true;
            return Ok(result);
        }

    }
}
