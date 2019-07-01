using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckRemainLessonsController : BasicController
    {
        public CheckRemainLessonsController(ablemusicContext ablemusicContext, ILogger<LessonRescheduleController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/CheckRemainLessons/5
        [HttpGet("{orgIDsStr}/{termId}")]
        public async Task<IActionResult> Get(string orgIDsStr, int termId)
        {
            var result = new Result<Dictionary<string, object>>();
            var orgIDsConvertResult = orgIDsStr.ToListOfID();
            if (!orgIDsConvertResult.IsSuccess)
            {
                return BadRequest(orgIDsConvertResult);
            }
            var orgIDs = orgIDsConvertResult.Data;            

            dynamic invoices;
            List<LessonRemain> lessonRemains;
            List<Learner> learners;
            dynamic otoLessons;
            dynamic gLessons;
            try
            {
                invoices = await (from i in _ablemusicContext.Invoice
                                  join l in _ablemusicContext.Learner on i.LearnerId equals l.LearnerId
                                  where orgIDs.Contains((int)l.OrgId) && i.TermId == termId
                                  select new
                                  {
                                      LearnerId = l.LearnerId,
                                      l.FirstName,
                                      l.LastName,
                                      InvoiceId = i.InvoiceId,
                                      PaidLesson = i.LessonQuantity,
                                      TermId = i.TermId,
                                      CourseInstanceId = i.CourseInstanceId,
                                      GroupCourseInstanceId = i.GroupCourseInstanceId,
                                  }).ToListAsync();
                var invoiceNums = new List<int>();
                foreach(var invoice in invoices)
                {
                    invoiceNums.Add(invoice.InvoiceId);
                }
                lessonRemains = await _ablemusicContext.LessonRemain.Where(lr => lr.TermId == termId).ToListAsync();
                learners = await _ablemusicContext.Learner.Where(l => orgIDs.Contains((int)l.OrgId)).ToListAsync();
                otoLessons = await (from l in _ablemusicContext.Lesson
                                 join oto in _ablemusicContext.One2oneCourseInstance on l.CourseInstanceId equals oto.CourseInstanceId
                                 join c in _ablemusicContext.Course on oto.CourseId equals c.CourseId
                                 where l.InvoiceId.HasValue && invoiceNums.Contains(l.InvoiceId.Value) && l.CourseInstanceId.HasValue
                                 select new 
                                 {
                                     l.LearnerId,
                                     CourseName = c.CourseName,
                                     c.CourseId,
                                     l.IsCanceled,
                                     l.IsConfirm,
                                     l.InvoiceId
                                 }).ToListAsync();
                gLessons = await (from l in _ablemusicContext.Lesson
                                  join gc in _ablemusicContext.GroupCourseInstance on l.GroupCourseInstanceId equals gc.GroupCourseInstanceId
                                  join c in _ablemusicContext.Course on gc.CourseId equals c.CourseId
                                  where l.GroupCourseInstanceId.HasValue && invoiceNums.Contains(l.InvoiceId.Value) && l.InvoiceId.HasValue
                                  select new
                                  {
                                      l.LearnerId,
                                      CourseName = c.CourseName,
                                      c.CourseId,
                                      l.IsCanceled,
                                      l.IsConfirm,
                                      l.InvoiceId
                                  }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            result.Data = new Dictionary<string, object>();

            foreach(var i in invoices)
            {
                int CourseId = 0;
                string CourseName = "";
                int PaidLessonInTotal = i.PaidLesson ?? 0;
                int ConfirmedLessonInTotal = 0;
                int RemainLessonInTotal = (from lr in lessonRemains
                                    where lr.TermId == i.TermId && lr.LearnerId == i.LearnerId && lr.CourseInstanceId == i.CourseInstanceId &&
                                    lr.GroupCourseInstanceId == i.GroupCourseInstanceId
                                    select lr.Quantity.Value).FirstOrDefault();
                int ArrangedLessonInTotal = 0;
                
                foreach (var otol in otoLessons)
                {
                    if (otol.InvoiceId == i.InvoiceId)
                    {
                        if(otol.IsConfirm == 1)
                        {
                            ConfirmedLessonInTotal++;
                        }
                        ArrangedLessonInTotal++;
                        CourseName = otol.CourseName;
                        CourseId = otol.CourseId;
                    }
                }
                foreach(var gl in gLessons)
                {
                    if(gl.InvoiceId == i.InvoiceId)
                    {
                        if(gl.IsConfirm == 1)
                        {
                            ConfirmedLessonInTotal++;
                        }
                        ArrangedLessonInTotal++;
                        CourseName = gl.CourseName;
                        CourseId = gl.CourseId;
                    }
                }
                result.Data.Add("InvoicID: " + i.InvoiceId.ToString(), new
                {
                    i.LearnerId,
                    LearnerName = i.FirstName + " " + i.LastName,
                    CourseId,
                    CourseName,
                    PaidLessonInTotal,
                    ConfirmedLessonInTotal,
                    RemainLessonInTotal,
                    ArrangedLessonInTotal,
                });
            }
            return Ok(result);
        }
    }
}
