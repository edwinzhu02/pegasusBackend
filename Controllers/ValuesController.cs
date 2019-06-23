﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : BasicController
    {
        readonly ILogger<ValuesController> _log;

        public ValuesController(ILogger<ValuesController> log)
        {
            _log = log;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            //_log.LogInformation("Hello, world!");
            return Ok(toNZTimezone(DateTime.UtcNow));
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
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