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
            //List<InvoiceWaitingConfirm> invoiceWaitingConfirms1;
            //List<InvoiceWaitingConfirm> invoiceWaitingConfirms2;
            //DateTime firstExecutionStart;
            //DateTime firstExecutionEnd;
            //DateTime secondExecutionStart;
            //DateTime secondExecutionEnd;

            try
            {
                //firstExecutionStart = DateTime.Now;
                //invoiceWaitingConfirms1 = await (from s in _ablemusicContext.Staff
                //                                join so in _ablemusicContext.StaffOrg on s.StaffId equals so.StaffId
                //                                join l in _ablemusicContext.Learner on so.OrgId equals l.OrgId
                //                                join iw in _ablemusicContext.InvoiceWaitingConfirm on l.LearnerId equals iw.LearnerId
                //                                join t in _ablemusicContext.Term on iw.TermId equals t.TermId
                //                                where s.UserId == 3 && toNZTimezone(DateTime.UtcNow).Date < t.EndDate && iw.IsActivate == 1
                //                                select new InvoiceWaitingConfirm
                //                                {
                //                                    WaitingId = iw.WaitingId,
                //                                    InvoiceNum = iw.InvoiceNum,
                //                                    LessonFee = iw.LessonFee,
                //                                    ConcertFee = iw.ConcertFee,
                //                                    NoteFee = iw.NoteFee,
                //                                    Other1Fee = iw.Other1Fee,
                //                                    Other2Fee = iw.Other2Fee,
                //                                    Other3Fee = iw.Other3Fee,
                //                                    LearnerId = iw.LearnerId,
                //                                    LearnerName = iw.LearnerName,
                //                                    BeginDate = iw.BeginDate,
                //                                    EndDate = iw.EndDate,
                //                                    TotalFee = iw.TotalFee,
                //                                    DueDate = iw.DueDate,
                //                                    PaidFee = iw.PaidFee,
                //                                    OwingFee = iw.OwingFee,
                //                                    CreatedAt = iw.CreatedAt,
                //                                    IsPaid = iw.IsPaid,
                //                                    TermId = iw.TermId,
                //                                    CourseInstanceId = iw.CourseInstanceId,
                //                                    GroupCourseInstanceId = iw.GroupCourseInstanceId,
                //                                    LessonQuantity = iw.LessonQuantity,
                //                                    CourseName = iw.CourseName,
                //                                    ConcertFeeName = iw.ConcertFeeName,
                //                                    LessonNoteFeeName = iw.LessonNoteFeeName,
                //                                    Other1FeeName = iw.Other1FeeName,
                //                                    Other2FeeName = iw.Other2FeeName,
                //                                    Other3FeeName = iw.Other3FeeName,
                //                                    IsActivate = iw.IsActivate,
                //                                    IsEmailSent = iw.IsEmailSent,
                //                                    IsConfirmed = iw.IsConfirmed,
                //                                    Learner = new Learner
                //                                    {
                //                                        Email = iw.Learner.Email,
                //                                        FirstName = iw.Learner.FirstName,
                //                                        MiddleName = iw.Learner.MiddleName,
                //                                        LastName = iw.Learner.LastName,
                //                                        EnrollDate = iw.Learner.EnrollDate,
                //                                        ContactNum = iw.Learner.ContactNum,
                //                                        Address = iw.Learner.Address,
                //                                        IsUnder18 = iw.Learner.IsUnder18,
                //                                        Dob = iw.Learner.Dob,
                //                                        Gender = iw.Learner.Gender,
                //                                        IsAbrsmG5 = iw.Learner.IsAbrsmG5,
                //                                        G5Certification = iw.Learner.G5Certification,
                //                                        CreatedAt = iw.Learner.CreatedAt,
                //                                        ReferrerLearnerId = iw.Learner.ReferrerLearnerId,
                //                                        Photo = iw.Learner.Photo,
                //                                        Note = iw.Learner.Note,
                //                                        LevelType = iw.Learner.LevelType,
                //                                        UserId = iw.Learner.UserId,
                //                                        OrgId = iw.Learner.OrgId,
                //                                        Parent = iw.Learner.Parent,
                //                                    }
                //                                }).ToListAsync();
                //firstExecutionEnd = DateTime.Now;
                //secondExecutionStart = DateTime.Now;
                //invoiceWaitingConfirms2 = await _ablemusicContext.InvoiceWaitingConfirm.Include(iw => iw.Term)
                //                                                                       .Include(iw => iw.Learner)
                //                                                                       .ThenInclude(o => o.Org)
                //                                                                       .ThenInclude(so => so.StaffOrg)
                //                                                                       .ThenInclude(s => s.Staff)
                //                                                                       .Where(iw => iw.IsActivate == 1)
                                                                                       
                //                                                                       .ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }


            //var arg = new NotificationEventArgs("Jesse","Say Hi","Details",1);
            //_notificationObservable.send(arg);

            //try
            //{
            //    throw new Exception();
            //}
            //catch(Exception ex)
            //{
            //    LogErrorToFile(ex.ToString());
            //}

            //LogInfoToFile("hello");
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