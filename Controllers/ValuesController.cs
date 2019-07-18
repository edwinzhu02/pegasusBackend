using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;
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
        public IActionResult Get()
        {
            Result<object> result = new Result<object>();
            var arg = new NotificationEventArgs("Jesse", "Say Hi", "Details", 1);
            _notificationObservable.send(arg);
            LogInfoToFile("hello");
            //throw new Exception("test exception");
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

        private Task AddLesson(ILogger log)
        {
            return Task.Factory.StartNew(() =>
            {
                var ablemusicContext = new ablemusicContext();
                DateTime startTime = new DateTime(2019, 10, 01, 08, 0, 0);
                for (int i = 0; i < 400; i++)
                {
                    startTime = startTime.AddDays(1);
                    for(int j = 0; j < 20; j++)
                    {
                        Lesson lesson = new Lesson
                        {
                            LearnerId = 10080,
                            RoomId = 37,
                            TeacherId = 256,
                            OrgId = 5,
                            IsCanceled = 0,
                            Reason = "8000 lessons for loading balance testing",
                            CreatedAt = DateTime.UtcNow.ToNZTimezone(),
                            CourseInstanceId = 10136,
                            GroupCourseInstanceId = null,
                            IsTrial = 0,
                            BeginTime = startTime.AddMinutes(30 * j),
                            EndTime = startTime.AddMinutes(30 * j + 30),
                            InvoiceId = 551,
                            IsConfirm = 0,
                            TrialCourseId = null,
                            IsChanged = 0,
                            IsPaid = 1,
                            NewLessonId = null,
                        };
                        ablemusicContext.Add(lesson);
                        log.LogInformation(j + " lessons has been added");
                    }
                    ablemusicContext.SaveChanges();
                    log.LogInformation(i + " 0 lessons has been saved to database");
                }
                log.LogInformation("completed");
            });
        }
    }
}