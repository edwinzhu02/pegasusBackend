using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : BasicController
    {
        public ValuesController(ablemusicContext ablemusicContext, ILogger<ValuesController> log) : base(ablemusicContext, log)
        {
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            var models = new List<InvoicePdfGeneratorModel>();
            models.Add(new InvoicePdfGeneratorModel{title = "Hello",amount = 100});
            InvoiceGenerator("Able music School","Oliver Deng",100,models,"oliverInvoice");
            return Ok();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return Ok();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}