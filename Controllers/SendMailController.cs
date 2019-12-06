using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendMailController : BasicController
    {
        public SendMailController(ablemusicContext ablemusicContext, ILogger<OrgsController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/Orgs
        [HttpPost]
        [CheckModelFilter]        
        public async Task<IActionResult> SendMail([FromForm] string Mail,[FromForm(Name="Attachment")] IFormFile Attachment)
        {
            Result<string> result = new Result<string>();
            try {
                var mail = JsonConvert.DeserializeObject<Mail>(Mail);
                MailSenderService.SendMailWithAttach(mail.MailTo,mail.MailTitle,mail.MailContent,Attachment);
                result.Data = "sucess";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
    }
}