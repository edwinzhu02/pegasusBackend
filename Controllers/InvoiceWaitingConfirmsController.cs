using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceWaitingConfirmsController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _ablemusicContext;
        private readonly IMapper _mapper;

        public InvoiceWaitingConfirmsController(pegasusContext.ablemusicContext ablemusicContext, IMapper mapper)
        {
            _ablemusicContext = ablemusicContext;
            _mapper = mapper;
        }

        // GET: api/InvoiceWaitingConfirms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceWaitingConfirm(int id)
        {
            Result<List<InvoiceWaitingConfirm>> result = new Result<List<InvoiceWaitingConfirm>>();
            List<InvoiceWaitingConfirm> invoiceWaitingConfirmsForOTOCourse = new List<InvoiceWaitingConfirm>();
            List<InvoiceWaitingConfirm> invoiceWaitingConfirmsForGroupCourse = new List<InvoiceWaitingConfirm>();
            try
            {
                result.Data = await (from s in _ablemusicContext.Staff
                                                            join so in _ablemusicContext.StaffOrg on s.StaffId equals so.StaffId
                                                            join l in _ablemusicContext.Learner on so.OrgId equals l.OrgId
                                                            join iw in _ablemusicContext.InvoiceWaitingConfirm on l.LearnerId equals iw.LearnerId
                                                            join t in _ablemusicContext.Term on iw.TermId equals t.TermId
                                                            where s.UserId == id && DateTime.Now.Date < t.EndDate && iw.IsActivate == 1
                                                            select new InvoiceWaitingConfirm
                                                            {
                                                                WaitingId = iw.WaitingId,
                                                                InvoiceNum = iw.InvoiceNum,
                                                                LessonFee = iw.LessonFee,
                                                                ConcertFee = iw.ConcertFee,
                                                                NoteFee = iw.NoteFee,
                                                                Other1Fee = iw.Other1Fee,
                                                                Other2Fee = iw.Other2Fee,
                                                                Other3Fee = iw.Other3Fee,
                                                                LearnerId = iw.LearnerId,
                                                                LearnerName = iw.LearnerName,
                                                                BeginDate = iw.BeginDate,
                                                                EndDate = iw.EndDate,
                                                                TotalFee = iw.TotalFee,
                                                                DueDate = iw.DueDate,
                                                                PaidFee = iw.PaidFee,
                                                                OwingFee = iw.OwingFee,
                                                                CreatedAt = iw.CreatedAt,
                                                                IsPaid = iw.IsPaid,
                                                                TermId = iw.TermId,
                                                                CourseInstanceId = iw.CourseInstanceId,
                                                                GroupCourseInstanceId = iw.GroupCourseInstanceId,
                                                                LessonQuantity = iw.LessonQuantity,
                                                                CourseName = iw.CourseName,
                                                                ConcertFeeName = iw.ConcertFeeName,
                                                                LessonNoteFeeName = iw.LessonNoteFeeName,
                                                                Other1FeeName = iw.Other1FeeName,
                                                                Other2FeeName = iw.Other2FeeName,
                                                                Other3FeeName = iw.Other3FeeName,
                                                                IsActivate = iw.IsActivate,
                                                                IsEmailSent = iw.IsEmailSent,
                                                                IsConfirmed = iw.IsConfirmed,
                                                                Learner = new Learner
                                                                {
                                                                    Email = iw.Learner.Email,
                                                                    FirstName = iw.Learner.FirstName,
                                                                    MiddleName = iw.Learner.MiddleName,
                                                                    LastName = iw.Learner.LastName,
                                                                    EnrollDate = iw.Learner.EnrollDate,
                                                                    ContactNum = iw.Learner.ContactNum,
                                                                    Address = iw.Learner.Address,
                                                                    IsUnder18 = iw.Learner.IsUnder18,
                                                                    Dob = iw.Learner.Dob,
                                                                    Gender = iw.Learner.Gender,
                                                                    IsAbrsmG5 = iw.Learner.IsAbrsmG5,
                                                                    G5Certification = iw.Learner.G5Certification,
                                                                    CreatedAt = iw.Learner.CreatedAt,
                                                                    ReferrerLearnerId = iw.Learner.ReferrerLearnerId,
                                                                    Photo = iw.Learner.Photo,
                                                                    Note = iw.Learner.Note,
                                                                    LevelType = iw.Learner.LevelType,
                                                                    UserId = iw.Learner.UserId,
                                                                    OrgId = iw.Learner.OrgId,
                                                                    Parent = iw.Learner.Parent,
                                                                }
                                                            }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }

        // PUT: api/InvoiceWaitingConfirms/5
        [HttpPut]
        [CheckModelFilter]
        public async Task<IActionResult> PutInvoiceWaitingConfirm([FromBody] InvoiceWaitingConfirmViewModel invoiceWaitingConfirmViewModel)
        {
            var result = new Result<string>();
            InvoiceWaitingConfirm invoiceWaitingConfirm = new InvoiceWaitingConfirm();
            InvoiceWaitingConfirm invoiceWaitingConfirmUpdate = new InvoiceWaitingConfirm();
            Invoice invoice = new Invoice();
            Learner learner = new Learner();
            _mapper.Map(invoiceWaitingConfirmViewModel, invoiceWaitingConfirm);
            try
            {
                invoiceWaitingConfirmUpdate = await _ablemusicContext.InvoiceWaitingConfirm.Where(i => i.WaitingId == invoiceWaitingConfirm.WaitingId).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = ex.Message;
                return NotFound(result);
            }
            
            if(invoiceWaitingConfirmUpdate == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "invoice waiting id not found";
                return NotFound(result);
            }
            if(invoiceWaitingConfirmUpdate.IsActivate == 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "The provided invoice id is not active";
                return BadRequest(result);
            }

            invoiceWaitingConfirmUpdate.IsActivate = 0;
            invoiceWaitingConfirm.IsConfirmed = 1;

            invoice.InvoiceNum = invoiceWaitingConfirmUpdate.InvoiceNum;
            invoice.LessonFee = invoiceWaitingConfirmUpdate.LessonFee;
            invoice.ConcertFee = invoiceWaitingConfirmUpdate.ConcertFee;
            invoice.NoteFee = invoiceWaitingConfirmUpdate.NoteFee;
            invoice.Other1Fee = invoiceWaitingConfirmUpdate.Other1Fee;
            invoice.Other2Fee = invoiceWaitingConfirmUpdate.Other2Fee;
            invoice.Other3Fee = invoiceWaitingConfirmUpdate.Other3Fee;
            invoice.LearnerId = invoiceWaitingConfirmUpdate.LearnerId;
            invoice.LearnerName = invoiceWaitingConfirmUpdate.LearnerName;
            invoice.BeginDate = invoiceWaitingConfirmUpdate.BeginDate;
            invoice.EndDate = invoiceWaitingConfirmUpdate.EndDate;
            invoice.TotalFee = invoiceWaitingConfirmUpdate.TotalFee;
            invoice.DueDate = invoiceWaitingConfirmUpdate.DueDate;
            invoice.PaidFee = invoiceWaitingConfirmUpdate.PaidFee;
            invoice.OwingFee = invoiceWaitingConfirmUpdate.OwingFee;
            invoice.CreatedAt = invoiceWaitingConfirmUpdate.CreatedAt;
            invoice.IsPaid = invoiceWaitingConfirmUpdate.IsPaid;
            invoice.TermId = invoiceWaitingConfirmUpdate.TermId;
            invoice.CourseInstanceId = invoiceWaitingConfirmUpdate.CourseInstanceId;
            invoice.GroupCourseInstanceId = invoiceWaitingConfirmUpdate.GroupCourseInstanceId;
            invoice.LessonQuantity = invoiceWaitingConfirmUpdate.LessonQuantity;
            invoice.CourseName = invoiceWaitingConfirmUpdate.CourseName;
            invoice.ConcertFeeName = invoiceWaitingConfirmUpdate.ConcertFeeName;
            invoice.LessonNoteFeeName = invoiceWaitingConfirmUpdate.LessonNoteFeeName;
            invoice.Other1FeeName = invoiceWaitingConfirmUpdate.Other1FeeName;
            invoice.Other2FeeName = invoiceWaitingConfirmUpdate.Other2FeeName;
            invoice.Other3FeeName = invoiceWaitingConfirmUpdate.Other3FeeName;

            try
            {
                await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoiceWaitingConfirm);
                await _ablemusicContext.Invoice.AddAsync(invoice);
                await _ablemusicContext.SaveChangesAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == invoice.LearnerId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            //sending Email
            string mailTitle = "Invoice";
            string mailContent = MailContentGenerator(invoiceWaitingConfirmUpdate);
            Task learnerMailSenderTask = MailSenderService.SendMailUpdateInvoiceWaitingTableAsync(learner.Email, mailTitle, mailContent, invoiceWaitingConfirmUpdate.WaitingId);

            return Ok(result);
        }

        private string MailContentGenerator(InvoiceWaitingConfirm invoiceWaitingConfirmUpdate)
        {
            string mailContent = "<div><p>Dear " + invoiceWaitingConfirmUpdate.LearnerName + ":</p>" + "<p>Here is the invoice for your" +
                    invoiceWaitingConfirmUpdate.CourseName + " lesson from " + invoiceWaitingConfirmUpdate.BeginDate.ToString() +
                    " to " + invoiceWaitingConfirmUpdate.EndDate.ToString() + "</p>" + "<p>Invoice Number: " +
                    invoiceWaitingConfirmUpdate.InvoiceNum + "</p>" + "<p>Lesson fee: " + invoiceWaitingConfirmUpdate.LessonFee +
                    "</p><p>Concert fee: " + invoiceWaitingConfirmUpdate.ConcertFee + "</p><p>Note fee " +
                    invoiceWaitingConfirmUpdate.NoteFee + "</p><p>Other fee: " + (invoiceWaitingConfirmUpdate.Other1Fee +
                    invoiceWaitingConfirmUpdate.Other2Fee + invoiceWaitingConfirmUpdate.Other3Fee).ToString() + "</p><p> Total fee " +
                    invoiceWaitingConfirmUpdate.TotalFee + "</p><p>Due date: " + invoiceWaitingConfirmUpdate.DueDate.ToString() + 
                    "</p><p>Paid fee: " + invoiceWaitingConfirmUpdate.PaidFee + "</p><p>Owing fee: " + invoiceWaitingConfirmUpdate.OwingFee + 
                    "</p>";
            return mailContent;
        }

        private bool InvoiceWaitingConfirmExists(int id)
        {
            return _ablemusicContext.InvoiceWaitingConfirm.Any(e => e.WaitingId == id);
        }
    }
}