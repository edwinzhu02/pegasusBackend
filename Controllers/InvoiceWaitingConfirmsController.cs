using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceWaitingConfirmsController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;

        public InvoiceWaitingConfirmsController(pegasusContext.ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }

        // GET: api/InvoiceWaitingConfirms
        [HttpGet]
        public IEnumerable<InvoiceWaitingConfirm> GetInvoiceWaitingConfirm()
        {
            return _ablemusicContext.InvoiceWaitingConfirm;
        }

        // GET: api/InvoiceWaitingConfirms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceWaitingConfirm(int userId)
        {
            List<InvoiceWaitingConfirm> invoiceWaitingConfirms = new List<InvoiceWaitingConfirm>();
            invoiceWaitingConfirms = await _ablemusicContext.InvoiceWaitingConfirm.Include(c => c.CourseInstance).Include(g => g.GroupCourseInstance).Include(l => l.Learner).Include(t => t.Term).ToListAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invoiceWaitingConfirm = await _ablemusicContext.InvoiceWaitingConfirm.FindAsync(userId);

            if (invoiceWaitingConfirm == null)
            {
                return NotFound();
            }

            return Ok(invoiceWaitingConfirms);
        }

        // PUT: api/InvoiceWaitingConfirms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceWaitingConfirm([FromRoute] int id, [FromBody] InvoiceWaitingConfirm invoiceWaitingConfirm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != invoiceWaitingConfirm.WaitingId)
            {
                return BadRequest();
            }

            _ablemusicContext.Entry(invoiceWaitingConfirm).State = EntityState.Modified;

            try
            {
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceWaitingConfirmExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool InvoiceWaitingConfirmExists(int id)
        {
            return _ablemusicContext.InvoiceWaitingConfirm.Any(e => e.WaitingId == id);
        }
    }
}