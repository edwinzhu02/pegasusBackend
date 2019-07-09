using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IMapper _mapper;

        public InvoiceWaitingConfirmsController(ablemusicContext ablemusicContext, ILogger<InvoiceWaitingConfirmsController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        // GET: api/InvoiceWaitingConfirms/5
        [HttpGet("{userId}/{termId}")]
        public async Task<IActionResult> GetInvoiceWaitingConfirm(int userId, int termId)
        {
            Result<List<object>> result = new Result<List<object>>();
            result.Data = new List<object>();
            List<InvoiceWaitingConfirm> invoiceWaitingConfirms = new List<InvoiceWaitingConfirm>();
            List<Invoice> invoices = new List<Invoice>();
            try
            {
                invoiceWaitingConfirms = await (from s in _ablemusicContext.Staff
                                                            join so in _ablemusicContext.StaffOrg on s.StaffId equals so.StaffId
                                                            join l in _ablemusicContext.Learner on so.OrgId equals l.OrgId
                                                            join iw in _ablemusicContext.InvoiceWaitingConfirm on l.LearnerId equals iw.LearnerId
                                                            join t in _ablemusicContext.Term on iw.TermId equals t.TermId
                                                            where s.UserId == userId && toNZTimezone(DateTime.UtcNow).Date < t.EndDate && iw.IsActivate == 1 && t.TermId == termId
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
                invoices = await (from s in _ablemusicContext.Staff
                                  join so in _ablemusicContext.StaffOrg on s.StaffId equals so.StaffId
                                  join l in _ablemusicContext.Learner on so.OrgId equals l.OrgId
                                  join i in _ablemusicContext.Invoice on l.LearnerId equals i.LearnerId
                                  join t in _ablemusicContext.Term on i.TermId equals t.TermId
                                  where s.UserId == userId && toNZTimezone(DateTime.UtcNow).Date < t.EndDate && i.IsActive == 1 && t.TermId == termId
                                  select new Invoice
                                  {
                                      InvoiceId = i.InvoiceId,
                                      InvoiceNum = i.InvoiceNum,
                                      LessonFee = i.LessonFee,
                                      ConcertFee = i.ConcertFee,
                                      NoteFee = i.NoteFee,
                                      Other1Fee = i.Other1Fee,
                                      Other2Fee = i.Other2Fee,
                                      Other3Fee = i.Other3Fee,
                                      LearnerId = i.LearnerId,
                                      LearnerName = i.LearnerName,
                                      BeginDate = i.BeginDate,
                                      EndDate = i.EndDate,
                                      TotalFee = i.TotalFee,
                                      DueDate = i.DueDate,
                                      PaidFee = i.PaidFee,
                                      OwingFee = i.OwingFee,
                                      CreatedAt = i.CreatedAt,
                                      IsPaid = i.IsPaid,
                                      TermId = i.TermId,
                                      CourseInstanceId = i.CourseInstanceId,
                                      GroupCourseInstanceId = i.GroupCourseInstanceId,
                                      LessonQuantity = i.LessonQuantity,
                                      CourseName = i.CourseName,
                                      ConcertFeeName = i.ConcertFeeName,
                                      LessonNoteFeeName = i.LessonNoteFeeName,
                                      Other1FeeName = i.Other1FeeName,
                                      Other2FeeName = i.Other2FeeName,
                                      Other3FeeName = i.Other3FeeName,
                                      IsActive = i.IsActive,
                                      Learner = new Learner
                                      {
                                          Email = i.Learner.Email,
                                          FirstName = i.Learner.FirstName,
                                          MiddleName = i.Learner.MiddleName,
                                          LastName = i.Learner.LastName,
                                          EnrollDate = i.Learner.EnrollDate,
                                          ContactNum = i.Learner.ContactNum,
                                          Address = i.Learner.Address,
                                          IsUnder18 = i.Learner.IsUnder18,
                                          Dob = i.Learner.Dob,
                                          Gender = i.Learner.Gender,
                                          IsAbrsmG5 = i.Learner.IsAbrsmG5,
                                          G5Certification = i.Learner.G5Certification,
                                          CreatedAt = i.Learner.CreatedAt,
                                          ReferrerLearnerId = i.Learner.ReferrerLearnerId,
                                          Photo = i.Learner.Photo,
                                          Note = i.Learner.Note,
                                          LevelType = i.Learner.LevelType,
                                          UserId = i.Learner.UserId,
                                          OrgId = i.Learner.OrgId,
                                          Parent = i.Learner.Parent,
                                      }
                                  }).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            foreach(var iw in invoiceWaitingConfirms)
            {
                dynamic currentInvoice;
                if (iw.IsConfirmed == 1)
                {
                    var i = invoices.Find(invoice => invoice.InvoiceNum == iw.InvoiceNum);
                    currentInvoice = new
                    {
                        InvoiceId = i.InvoiceId,
                        InvoiceNum = i.InvoiceNum,
                        LessonFee = i.LessonFee,
                        ConcertFee = i.ConcertFee,
                        NoteFee = i.NoteFee,
                        Other1Fee = i.Other1Fee,
                        Other2Fee = i.Other2Fee,
                        Other3Fee = i.Other3Fee,
                        LearnerId = i.LearnerId,
                        LearnerName = i.LearnerName,
                        BeginDate = i.BeginDate,
                        EndDate = i.EndDate,
                        TotalFee = i.TotalFee,
                        DueDate = i.DueDate,
                        PaidFee = i.PaidFee,
                        OwingFee = i.OwingFee,
                        CreatedAt = i.CreatedAt,
                        IsPaid = i.IsPaid,
                        TermId = i.TermId,
                        CourseInstanceId = i.CourseInstanceId,
                        GroupCourseInstanceId = i.GroupCourseInstanceId,
                        LessonQuantity = i.LessonQuantity,
                        CourseName = i.CourseName,
                        ConcertFeeName = i.ConcertFeeName,
                        LessonNoteFeeName = i.LessonNoteFeeName,
                        Other1FeeName = i.Other1FeeName,
                        Other2FeeName = i.Other2FeeName,
                        Other3FeeName = i.Other3FeeName,
                        IsActive = i.IsActive,
                        Learner = new Learner
                        {
                            Email = i.Learner.Email,
                            FirstName = i.Learner.FirstName,
                            MiddleName = i.Learner.MiddleName,
                            LastName = i.Learner.LastName,
                            EnrollDate = i.Learner.EnrollDate,
                            ContactNum = i.Learner.ContactNum,
                            Address = i.Learner.Address,
                            IsUnder18 = i.Learner.IsUnder18,
                            Dob = i.Learner.Dob,
                            Gender = i.Learner.Gender,
                            IsAbrsmG5 = i.Learner.IsAbrsmG5,
                            G5Certification = i.Learner.G5Certification,
                            CreatedAt = i.Learner.CreatedAt,
                            ReferrerLearnerId = i.Learner.ReferrerLearnerId,
                            Photo = i.Learner.Photo,
                            Note = i.Learner.Note,
                            LevelType = i.Learner.LevelType,
                            UserId = i.Learner.UserId,
                            OrgId = i.Learner.OrgId,
                            Parent = i.Learner.Parent,
                        }
                    };
                } else
                {
                    currentInvoice = new
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
                    };
                    
                }
                result.Data.Add(currentInvoice);
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
            List<Invoice> activeInvoices = new List<Invoice>();
            Invoice invoice = new Invoice();
            Invoice existInvoice = new Invoice();
            Learner learner = new Learner();
            _mapper.Map(invoiceWaitingConfirmViewModel, invoiceWaitingConfirm);
            try
            {
                invoiceWaitingConfirmUpdate = await _ablemusicContext.InvoiceWaitingConfirm.Where(i => i.InvoiceNum == invoiceWaitingConfirm.InvoiceNum).FirstOrDefaultAsync();
                activeInvoices = await _ablemusicContext.Invoice.Where(i => (i.IsActive == 1 || i.IsActive == null) && i.InvoiceNum == invoiceWaitingConfirm.InvoiceNum).ToListAsync();
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
            if(activeInvoices.Count > 0 && activeInvoices.FirstOrDefault().PaidFee > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "The provided invoice is already paid";
                return BadRequest(result);
            }

            invoiceWaitingConfirmUpdate.IsActivate = 0;
            invoiceWaitingConfirm.IsConfirmed = 1;
            invoiceWaitingConfirm.WaitingId = 0;
            invoiceWaitingConfirm.IsPaid = 0;            
            invoiceWaitingConfirm.IsActivate = 1;
            invoiceWaitingConfirm.IsEmailSent = 0;
            invoiceWaitingConfirm.CreatedAt = toNZTimezone(DateTime.UtcNow);
            //for patch
            invoiceWaitingConfirm.EndDate = invoiceWaitingConfirmUpdate.EndDate;
            invoiceWaitingConfirm.DueDate = invoiceWaitingConfirmUpdate.DueDate;
            //

            invoice.InvoiceNum = invoiceWaitingConfirm.InvoiceNum;
            invoice.LessonFee = invoiceWaitingConfirm.LessonFee;
            invoice.ConcertFee = invoiceWaitingConfirm.ConcertFee;
            invoice.NoteFee = invoiceWaitingConfirm.NoteFee;
            invoice.Other1Fee = invoiceWaitingConfirm.Other1Fee;
            invoice.Other2Fee = invoiceWaitingConfirm.Other2Fee;
            invoice.Other3Fee = invoiceWaitingConfirm.Other3Fee;
            invoice.LearnerId = invoiceWaitingConfirm.LearnerId;
            invoice.LearnerName = invoiceWaitingConfirm.LearnerName;
            invoice.BeginDate = invoiceWaitingConfirm.BeginDate;
            invoice.EndDate = invoiceWaitingConfirm.EndDate;
            invoice.TotalFee = invoiceWaitingConfirm.TotalFee;
            invoice.DueDate = invoiceWaitingConfirm.DueDate;
            invoice.PaidFee = invoiceWaitingConfirm.PaidFee;
            invoice.OwingFee = invoiceWaitingConfirm.OwingFee;
            invoice.CreatedAt = invoiceWaitingConfirm.CreatedAt;
            invoice.IsPaid = invoiceWaitingConfirm.IsPaid;
            invoice.TermId = invoiceWaitingConfirm.TermId;
            invoice.CourseInstanceId = invoiceWaitingConfirm.CourseInstanceId;
            invoice.GroupCourseInstanceId = invoiceWaitingConfirm.GroupCourseInstanceId;
            invoice.LessonQuantity = invoiceWaitingConfirm.LessonQuantity;
            invoice.CourseName = invoiceWaitingConfirm.CourseName;
            invoice.ConcertFeeName = invoiceWaitingConfirm.ConcertFeeName;
            invoice.LessonNoteFeeName = invoiceWaitingConfirm.LessonNoteFeeName;
            invoice.Other1FeeName = invoiceWaitingConfirm.Other1FeeName;
            invoice.Other2FeeName = invoiceWaitingConfirm.Other2FeeName;
            invoice.Other3FeeName = invoiceWaitingConfirm.Other3FeeName;
            invoice.IsActive = 1;

            if (activeInvoices.Count > 0)
            {
                foreach(var activeInvoice in activeInvoices)
                {
                    activeInvoice.IsActive = 0;
                }
            }

            try
            {
                await _ablemusicContext.SaveChangesAsync();
                await _ablemusicContext.InvoiceWaitingConfirm.AddAsync(invoiceWaitingConfirm);
                await _ablemusicContext.Invoice.AddAsync(invoice);
                await _ablemusicContext.SaveChangesAsync();
                learner = await _ablemusicContext.Learner.Where(l => l.LearnerId == invoice.LearnerId).FirstOrDefaultAsync();
                if(learner == null)
                    {
                        result.IsSuccess = false;
                        result.IsFound = false;
                        result.ErrorMessage = "learner not found";
                        return NotFound(result);
                    }
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