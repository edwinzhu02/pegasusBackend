using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Pegasus_backend.Models;
using Microsoft.EntityFrameworkCore;


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
        public async Task<IActionResult> Get()
        {
            Result<object> result = new Result<object>();
            var arg = new NotificationEventArgs("Jesse", "Say Hi", "Details", 1);
            _notificationObservable.send(arg);

            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex.ToString());
            }

            LogInfoToFile("hello");
            return Ok(toNZTimezone(DateTime.UtcNow));
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