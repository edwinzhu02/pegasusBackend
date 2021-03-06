﻿using System;
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
using Pegasus_backend.Utilities;

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

        // GET: api/InvoiceWaitingConfirms/5/1
        [HttpGet("{userId}/{termId}")]
        public async Task<IActionResult> GetInvoiceWaitingConfirm(int userId, int termId)
        {
            Result<List<object>> result = new Result<List<object>>();
            result.Data = new List<object>();
            dynamic invoiceWaitingConfirms;
            List<Invoice> invoices = new List<Invoice>();
            try
            {
                invoiceWaitingConfirms = await (from s in _ablemusicContext.Staff
                                                join so in _ablemusicContext.StaffOrg on s.StaffId equals so.StaffId
                                                join l in _ablemusicContext.Learner on so.OrgId equals l.OrgId
                                                join o in _ablemusicContext.Org on l.OrgId  equals  o.OrgId                                              
                                                join iw in _ablemusicContext.InvoiceWaitingConfirm on l.LearnerId equals iw.LearnerId
                                                join t in _ablemusicContext.Term on iw.TermId equals t.TermId
                                                join iv in _ablemusicContext.Invoice on iw.InvoiceNum equals iv.InvoiceNum into f
                                                from i in f.DefaultIfEmpty()
                                                where s.UserId == userId && t.TermId == termId && iw.IsActivate==1 && i.IsActive==1
                                                select new
                                                {
                                                    InvoiceNum = iw.InvoiceNum,
                                                    Learner = new
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
                                                        Org = iw.Learner.Org
                                                    },
                                                    InvoiceWaitingConfirm = new
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
                                                        Comment = iw.Comment,
                                                        Other4Fee = iw.Other4Fee,
                                                        Other4FeeName = iw.Other4FeeName,
                                                        Other5Fee = iw.Other5Fee,
                                                        Other5FeeName = iw.Other5FeeName,  
                                                        Other6Fee = iw.Other6Fee,
                                                        Other6FeeName = iw.Other6FeeName,
                                                        Other7Fee = iw.Other7Fee,
                                                        Other7FeeName = iw.Other7FeeName,   
                                                        Other8Fee = iw.Other8Fee,
                                                        Other8FeeName = iw.Other8FeeName,
                                                        Other9Fee = iw.Other9Fee,
                                                        Other9FeeName = iw.Other9FeeName,  
                                                        Other10Fee = iw.Other10Fee,
                                                        Other10FeeName = iw.Other10FeeName,
                                                        Other11Fee = iw.Other11Fee,
                                                        Other11FeeName = iw.Other11FeeName,                                                                                                             
                                                        Other12Fee = iw.Other12Fee,
                                                        Other12FeeName = iw.Other12FeeName,
                                                        Other13Fee = iw.Other13Fee,
                                                        Other13FeeName = iw.Other13FeeName,  
                                                        Other14Fee = iw.Other14Fee,
                                                        Other14FeeName = iw.Other14FeeName,
                                                        Other15Fee = iw.Other15Fee,
                                                        Other15FeeName = iw.Other15FeeName,                                                                                                             
                                                        Other16Fee = iw.Other16Fee,
                                                        Other16FeeName = iw.Other16FeeName,  
                                                        Other17Fee = iw.Other17Fee,
                                                        Other17FeeName = iw.Other17FeeName,
                                                        Other18Fee = iw.Other18Fee,
                                                        Other18FeeName = iw.Other18FeeName,   
                                                    },
                                                    Invoice = new 
                                                    {
                                                        InvoiceId = i == null ? 0 : i.InvoiceId,
                                                        InvoiceNum = i == null ? string.Empty : i.InvoiceNum,
                                                        LessonFee = i == null ? 0 : i.LessonFee,
                                                        ConcertFee = i == null ? 0 : i.ConcertFee,
                                                        NoteFee = i == null ? 0 : i.NoteFee,
                                                        Other1Fee = i == null ? 0 : i.Other1Fee,
                                                        Other2Fee = i == null ? 0 : i.Other2Fee,
                                                        Other3Fee = i == null ? 0 : i.Other3Fee,
                                                        Other4Fee = i == null ? 0 : i.Other4Fee,
                                                        Other5Fee = i == null ? 0 : i.Other5Fee,
                                                        Other6Fee = i == null ? 0 : i.Other6Fee,
                                                        Other7Fee = i == null ? 0 : i.Other7Fee,
                                                        Other8Fee = i == null ? 0 : i.Other8Fee,
                                                        Other9Fee = i == null ? 0 : i.Other9Fee,
                                                        Other10Fee = i == null ? 0 : i.Other10Fee,
                                                        Other11Fee = i == null ? 0 : i.Other11Fee,
                                                        Other12Fee = i == null ? 0 : i.Other12Fee,
                                                        Other13Fee = i == null ? 0 : i.Other13Fee,
                                                        Other14Fee = i == null ? 0 : i.Other14Fee,
                                                        Other15Fee = i == null ? 0 : i.Other15Fee,
                                                        Other16Fee = i == null ? 0 : i.Other16Fee,
                                                        Other17Fee = i == null ? 0 : i.Other17Fee,
                                                        Other18Fee = i == null ? 0 : i.Other18Fee,                                                                                                                                                                                                                                                                                        
                                                        LearnerId = i == null ? 0 : i.LearnerId,
                                                        LearnerName = i == null ? string.Empty : i.LearnerName,
                                                        BeginDate = i == null ? null : i.BeginDate,
                                                        EndDate = i == null ? null : i.EndDate,
                                                        TotalFee = i == null ? 0 : i.TotalFee,
                                                        DueDate = i == null ? null : i.DueDate,
                                                        PaidFee = i == null ? 0 : i.PaidFee,
                                                        OwingFee = i == null ? 0 : i.OwingFee,
                                                        CreatedAt = i == null ? null : i.CreatedAt,
                                                        IsPaid = i == null ? 0 : i.IsPaid,
                                                        TermId = i == null ? 0 : i.TermId,
                                                        CourseInstanceId = i == null ? 0 : i.CourseInstanceId,
                                                        GroupCourseInstanceId = i == null ? 0 : i.GroupCourseInstanceId,
                                                        LessonQuantity = i == null ? 0 : i.LessonQuantity,
                                                        CourseName = i == null ? string.Empty : i.CourseName,
                                                        ConcertFeeName = i == null ? string.Empty : i.ConcertFeeName,
                                                        LessonNoteFeeName = i == null ? string.Empty : i.LessonNoteFeeName,
                                                        Other1FeeName = i == null ? string.Empty : i.Other1FeeName,
                                                        Other2FeeName = i == null ? string.Empty : i.Other2FeeName,
                                                        Other3FeeName = i == null ? string.Empty : i.Other3FeeName,
                                                        Other4FeeName = i == null ? string.Empty : i.Other4FeeName,
                                                        Other5FeeName = i == null ? string.Empty : i.Other5FeeName,
                                                        Other6FeeName = i == null ? string.Empty : i.Other6FeeName,
                                                        Other7FeeName = i == null ? string.Empty : i.Other7FeeName,
                                                        Other8FeeName = i == null ? string.Empty : i.Other8FeeName,
                                                        Other9FeeName = i == null ? string.Empty : i.Other9FeeName,
                                                        Other10FeeName = i == null ? string.Empty : i.Other10FeeName,
                                                        Other11FeeName = i == null ? string.Empty : i.Other11FeeName,
                                                        Other12FeeName = i == null ? string.Empty : i.Other12FeeName,
                                                        Other13FeeName = i == null ? string.Empty : i.Other13FeeName,
                                                        Other14FeeName = i == null ? string.Empty : i.Other14FeeName,
                                                        Other15FeeName = i == null ? string.Empty : i.Other15FeeName,
                                                        Other16FeeName = i == null ? string.Empty : i.Other16FeeName,
                                                        Other17FeeName = i == null ? string.Empty : i.Other17FeeName,
                                                        Other18FeeName = i == null ? string.Empty : i.Other18FeeName,

                                                        IsActive = i == null ? 0 : i.IsActive,
                                                        Comment = i == null ? string.Empty : i.Comment
                                                    },
                                                }).ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            string preInvoiceNum = "";
            foreach(var i in invoiceWaitingConfirms)
            {
                string currentInvoiceNum = i.InvoiceWaitingConfirm.InvoiceNum;
                if (preInvoiceNum != currentInvoiceNum) 
                {
                    dynamic currentInvoices = new List<object>();
                    preInvoiceNum = currentInvoiceNum;
                    foreach(var i2 in invoiceWaitingConfirms)
                    {
                        if(i2.InvoiceWaitingConfirm.InvoiceNum == currentInvoiceNum)
                        {
                            currentInvoices.Add(i2);
                        }
                    }
                    if(currentInvoices.Count == 1)
                    {
                        result.Data.Add(currentInvoices[0]);
                    } else
                    {
                        int initialInvoiceWaitingId = int.MaxValue;
                        int latestInvoiceId = 0;
                        foreach (var ci in currentInvoices)
                        {
                            int currentWaitingId = ci.InvoiceWaitingConfirm.WaitingId;
                            int currentInvoiceId = ci.Invoice.InvoiceId;
                            initialInvoiceWaitingId = initialInvoiceWaitingId > currentWaitingId ? currentWaitingId : initialInvoiceWaitingId;
                            latestInvoiceId = latestInvoiceId < currentInvoiceId ? currentInvoiceId : latestInvoiceId;
                        }
                        foreach (var ci in currentInvoices)
                        {
                            int currentWaitingId = ci.InvoiceWaitingConfirm.WaitingId;
                            int currentInvoiceId = ci.Invoice.InvoiceId;

                            if (currentWaitingId == initialInvoiceWaitingId && latestInvoiceId == currentInvoiceId)
                            {
                                result.Data.Add(ci);
                            }
                        }
                    }
                    
                }
            }
            return Ok(result);
        }
        [HttpGet("{learnerId}")]
        public async Task<IActionResult> GetInvoiceByLearner(int learnerId)
        {
            Result<List<object>> result = new Result<List<object>>();
            result.Data = new List<object>();
            dynamic invoiceWaitingConfirms;
            List<Invoice> invoices = new List<Invoice>();
            try
            {
                
                invoiceWaitingConfirms = await (from //s in _ablemusicContext.Staff
                                                //join so in _ablemusicContext.StaffOrg on s.StaffId equals so.StaffId
                                                l in _ablemusicContext.Learner //on so.OrgId equals l.OrgId
                                                join o in _ablemusicContext.Org on l.OrgId equals o.OrgId
                                                join iw in _ablemusicContext.InvoiceWaitingConfirm on l.LearnerId equals iw.LearnerId
                                                //join t in _ablemusicContext.Term on iw.TermId equals t.TermId
                                                join iv in _ablemusicContext.Invoice on iw.InvoiceNum equals iv.InvoiceNum into f
                                                from i in f.DefaultIfEmpty()
                                                where l.LearnerId==learnerId && iw.IsActivate ==1 
                                                select new
                                                {
                                                    InvoiceNum = iw.InvoiceNum,
                                                    Learner = new
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
                                                        Org = iw.Learner.Org
                                                    },
                                                    InvoiceWaitingConfirm = new
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
                                                        Comment = iw.Comment,
                                                        Other4Fee = iw.Other4Fee,
                                                        Other4FeeName = iw.Other4FeeName,
                                                        Other5Fee = iw.Other5Fee,
                                                        Other5FeeName = iw.Other5FeeName,  
                                                        Other6Fee = iw.Other6Fee,
                                                        Other6FeeName = iw.Other6FeeName,
                                                        Other7Fee = iw.Other7Fee,
                                                        Other7FeeName = iw.Other7FeeName,   
                                                        Other8Fee = iw.Other8Fee,
                                                        Other8FeeName = iw.Other8FeeName,
                                                        Other9Fee = iw.Other9Fee,
                                                        Other9FeeName = iw.Other9FeeName,  
                                                        Other10Fee = iw.Other10Fee,
                                                        Other10FeeName = iw.Other10FeeName,
                                                        Other11Fee = iw.Other11Fee,
                                                        Other11FeeName = iw.Other11FeeName,                                                                                                             
                                                        Other12Fee = iw.Other12Fee,
                                                        Other12FeeName = iw.Other12FeeName,
                                                        Other13Fee = iw.Other13Fee,
                                                        Other13FeeName = iw.Other13FeeName,  
                                                        Other14Fee = iw.Other14Fee,
                                                        Other14FeeName = iw.Other14FeeName,
                                                        Other15Fee = iw.Other15Fee,
                                                        Other15FeeName = iw.Other15FeeName,                                                                                                             
                                                        Other16Fee = iw.Other16Fee,
                                                        Other16FeeName = iw.Other16FeeName,  
                                                        Other17Fee = iw.Other17Fee,
                                                        Other17FeeName = iw.Other17FeeName,
                                                        Other18Fee = iw.Other18Fee,
                                                        Other18FeeName = iw.Other18FeeName,                                                           
                                                    },
                                                    Invoice = new 
                                                    {
                                                        InvoiceId = i == null ? 0 : i.InvoiceId,
                                                        InvoiceNum = i == null ? string.Empty : i.InvoiceNum,
                                                        LessonFee = i == null ? 0 : i.LessonFee,
                                                        ConcertFee = i == null ? 0 : i.ConcertFee,
                                                        NoteFee = i == null ? 0 : i.NoteFee,
                                                        Other1Fee = i == null ? 0 : i.Other1Fee,
                                                        Other2Fee = i == null ? 0 : i.Other2Fee,
                                                        Other3Fee = i == null ? 0 : i.Other3Fee,
                                                        Other4Fee = i == null ? 0 : i.Other4Fee,
                                                        Other5Fee = i == null ? 0 : i.Other5Fee,
                                                        Other6Fee = i == null ? 0 : i.Other6Fee,
                                                        Other7Fee = i == null ? 0 : i.Other7Fee,
                                                        Other8Fee = i == null ? 0 : i.Other8Fee,
                                                        Other9Fee = i == null ? 0 : i.Other9Fee,
                                                        Other10Fee = i == null ? 0 : i.Other10Fee,
                                                        Other11Fee = i == null ? 0 : i.Other11Fee,
                                                        Other12Fee = i == null ? 0 : i.Other12Fee,
                                                        Other13Fee = i == null ? 0 : i.Other13Fee,
                                                        Other14Fee = i == null ? 0 : i.Other14Fee,
                                                        Other15Fee = i == null ? 0 : i.Other15Fee,
                                                        Other16Fee = i == null ? 0 : i.Other16Fee,
                                                        Other17Fee = i == null ? 0 : i.Other17Fee,
                                                        Other18Fee = i == null ? 0 : i.Other18Fee,                                                                                                                                                                                                                                                                                        
                                                        
                                                        LearnerId = i == null ? 0 : i.LearnerId,
                                                        LearnerName = i == null ? string.Empty : i.LearnerName,
                                                        BeginDate = i == null ? null : i.BeginDate,
                                                        EndDate = i == null ? null : i.EndDate,
                                                        TotalFee = i == null ? 0 : i.TotalFee,
                                                        DueDate = i == null ? null : i.DueDate,
                                                        PaidFee = i == null ? 0 : i.PaidFee,
                                                        OwingFee = i == null ? 0 : i.OwingFee,
                                                        CreatedAt = i == null ? null : i.CreatedAt,
                                                        IsPaid = i == null ? 0 : i.IsPaid,
                                                        TermId = i == null ? 0 : i.TermId,
                                                        CourseInstanceId = i == null ? 0 : i.CourseInstanceId,
                                                        GroupCourseInstanceId = i == null ? 0 : i.GroupCourseInstanceId,
                                                        LessonQuantity = i == null ? 0 : i.LessonQuantity,
                                                        CourseName = i == null ? string.Empty : i.CourseName,
                                                        ConcertFeeName = i == null ? string.Empty : i.ConcertFeeName,
                                                        LessonNoteFeeName = i == null ? string.Empty : i.LessonNoteFeeName,
                                                        Other1FeeName = i == null ? string.Empty : i.Other1FeeName,
                                                        Other2FeeName = i == null ? string.Empty : i.Other2FeeName,
                                                        Other3FeeName = i == null ? string.Empty : i.Other3FeeName,
                                                        Other4FeeName = i == null ? string.Empty : i.Other4FeeName,
                                                        Other5FeeName = i == null ? string.Empty : i.Other5FeeName,
                                                        Other6FeeName = i == null ? string.Empty : i.Other6FeeName,
                                                        Other7FeeName = i == null ? string.Empty : i.Other7FeeName,
                                                        Other8FeeName = i == null ? string.Empty : i.Other8FeeName,
                                                        Other9FeeName = i == null ? string.Empty : i.Other9FeeName,
                                                        Other10FeeName = i == null ? string.Empty : i.Other10FeeName,
                                                        Other11FeeName = i == null ? string.Empty : i.Other11FeeName,
                                                        Other12FeeName = i == null ? string.Empty : i.Other12FeeName,
                                                        Other13FeeName = i == null ? string.Empty : i.Other13FeeName,
                                                        Other14FeeName = i == null ? string.Empty : i.Other14FeeName,
                                                        Other15FeeName = i == null ? string.Empty : i.Other15FeeName,
                                                        Other16FeeName = i == null ? string.Empty : i.Other16FeeName,
                                                        Other17FeeName = i == null ? string.Empty : i.Other17FeeName,
                                                        Other18FeeName = i == null ? string.Empty : i.Other18FeeName,

                                                        IsActive = i == null ? 0 : i.IsActive,
                                                        Comment = i == null ? string.Empty : i.Comment
                                                    },
                                                }).OrderByDescending(re => re.InvoiceWaitingConfirm.InvoiceNum).ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
        
            string preInvoiceNum = "";
            DateTime dateNow = DateTime.UtcNow.ToNZTimezone();
            foreach(var i in invoiceWaitingConfirms)
            {
                string currentInvoiceNum = i.InvoiceWaitingConfirm.InvoiceNum;
                DateTime InvoiceEndDate =i.InvoiceWaitingConfirm.EndDate;
                TimeSpan ts = InvoiceEndDate.Subtract(dateNow);
                int totalWeeks = (int)(ts.Days / 7);
                if (totalWeeks>14+6) continue; //if invoice date more than 6 week, don't list to user.
                if (i.InvoiceWaitingConfirm.TotalFee == 0 ) continue; //if invoice date more than 6 week, don't list to user.

                if (preInvoiceNum != currentInvoiceNum) 
                {
                    dynamic currentInvoices = new List<object>();
                    preInvoiceNum = currentInvoiceNum;
                    foreach(var i2 in invoiceWaitingConfirms)
                    {
                        if(i2.InvoiceWaitingConfirm.InvoiceNum == currentInvoiceNum)
                        {
                            currentInvoices.Add(i2);
                        }
                    }
                    if(currentInvoices.Count == 1)
                    {
                        result.Data.Add(currentInvoices[0]);
                    } else
                    {
                        int initialInvoiceWaitingId = int.MaxValue;
                        int latestInvoiceId = 0;
                        foreach (var ci in currentInvoices)
                        {
                            int currentWaitingId = ci.InvoiceWaitingConfirm.WaitingId;
                            int currentInvoiceId = ci.Invoice.InvoiceId;
                            initialInvoiceWaitingId = initialInvoiceWaitingId > currentWaitingId ? currentWaitingId : initialInvoiceWaitingId;
                            latestInvoiceId = latestInvoiceId < currentInvoiceId ? currentInvoiceId : latestInvoiceId;
                        }
                        foreach (var ci in currentInvoices)
                        {
                            int currentWaitingId = ci.InvoiceWaitingConfirm.WaitingId;
                            int currentInvoiceId = ci.Invoice.InvoiceId;

                            if (currentWaitingId == initialInvoiceWaitingId && latestInvoiceId == currentInvoiceId)
                            {
                                result.Data.Add(ci);
                            }
                        }
                    }
                    
                }
            }
            return Ok(result);
        }
        // PUT: api/InvoiceWaitingConfirms
        [HttpPut]
        [CheckModelFilter]
        public async Task<IActionResult> PutInvoiceWaitingConfirm([FromBody] InvoiceWaitingConfirmViewModel invoiceWaitingConfirmViewModel)
        {
            var result = new Result<string>();
            InvoiceWaitingConfirm invoiceWaitingConfirm = new InvoiceWaitingConfirm();
            InvoiceWaitingConfirm invoiceWaitingConfirmUpdate = new InvoiceWaitingConfirm();
            List<Invoice> activeInvoices = new List<Invoice>();
            Invoice existInvoice = new Invoice();
            Learner learner = new Learner();
            _mapper.Map(invoiceWaitingConfirmViewModel, invoiceWaitingConfirm);
            try
            {
                invoiceWaitingConfirmUpdate = await _ablemusicContext.InvoiceWaitingConfirm.
                Where(i => (i.InvoiceNum == invoiceWaitingConfirm.InvoiceNum && i.IsActivate == 1)).FirstOrDefaultAsync();
                activeInvoices = await _ablemusicContext.Invoice.
                    Where(i => (i.IsActive == 1 || i.IsActive == null) 
                    && i.InvoiceNum == invoiceWaitingConfirm.InvoiceNum).ToListAsync();
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
            Invoice invoice = new Invoice
            {
                InvoiceNum = invoiceWaitingConfirm.InvoiceNum,
                LessonFee = invoiceWaitingConfirm.LessonFee,
                ConcertFee = invoiceWaitingConfirm.ConcertFee,
                NoteFee = invoiceWaitingConfirm.NoteFee,
                Other1Fee = invoiceWaitingConfirm.Other1Fee,
                Other2Fee = invoiceWaitingConfirm.Other2Fee,
                Other3Fee = invoiceWaitingConfirm.Other3Fee,
                Other4Fee = invoiceWaitingConfirm.Other4Fee,
                Other5Fee = invoiceWaitingConfirm.Other5Fee,
                Other6Fee = invoiceWaitingConfirm.Other6Fee,
                Other7Fee = invoiceWaitingConfirm.Other7Fee,
                Other8Fee = invoiceWaitingConfirm.Other8Fee,
                Other9Fee = invoiceWaitingConfirm.Other9Fee,
                Other10Fee = invoiceWaitingConfirm.Other10Fee,
                Other11Fee = invoiceWaitingConfirm.Other11Fee,
                Other12Fee = invoiceWaitingConfirm.Other12Fee,
                Other13Fee = invoiceWaitingConfirm.Other13Fee,
                Other14Fee = invoiceWaitingConfirm.Other14Fee,
                Other15Fee = invoiceWaitingConfirm.Other15Fee,    
                Other16Fee = invoiceWaitingConfirm.Other16Fee,
                Other17Fee = invoiceWaitingConfirm.Other17Fee,
                Other18Fee = invoiceWaitingConfirm.Other18Fee,                                                                              
                LearnerId = invoiceWaitingConfirm.LearnerId,
                LearnerName = invoiceWaitingConfirm.LearnerName,
                BeginDate = invoiceWaitingConfirm.BeginDate,
                EndDate = invoiceWaitingConfirm.EndDate,
                TotalFee = invoiceWaitingConfirm.TotalFee,
                DueDate = invoiceWaitingConfirm.DueDate,
                PaidFee = invoiceWaitingConfirm.PaidFee,
                OwingFee = invoiceWaitingConfirm.OwingFee,
                CreatedAt = invoiceWaitingConfirm.CreatedAt,
                IsPaid = invoiceWaitingConfirm.IsPaid,
                TermId = invoiceWaitingConfirm.TermId,
                CourseInstanceId = invoiceWaitingConfirm.CourseInstanceId,
                GroupCourseInstanceId = invoiceWaitingConfirm.GroupCourseInstanceId,
                LessonQuantity = invoiceWaitingConfirm.LessonQuantity,
                CourseName = invoiceWaitingConfirm.CourseName,
                ConcertFeeName = invoiceWaitingConfirm.ConcertFeeName,
                LessonNoteFeeName = invoiceWaitingConfirm.LessonNoteFeeName,
                Other1FeeName = invoiceWaitingConfirm.Other1FeeName,
                Other2FeeName = invoiceWaitingConfirm.Other2FeeName,
                Other3FeeName = invoiceWaitingConfirm.Other3FeeName,
                Other4FeeName = invoiceWaitingConfirm.Other4FeeName,
                Other5FeeName = invoiceWaitingConfirm.Other5FeeName,
                Other6FeeName = invoiceWaitingConfirm.Other6FeeName,
                Other7FeeName = invoiceWaitingConfirm.Other7FeeName,
                Other8FeeName = invoiceWaitingConfirm.Other8FeeName,
                Other9FeeName = invoiceWaitingConfirm.Other9FeeName,
                Other10FeeName = invoiceWaitingConfirm.Other10FeeName,
                Other11FeeName = invoiceWaitingConfirm.Other11FeeName,
                Other12FeeName = invoiceWaitingConfirm.Other12FeeName,
                Other13FeeName = invoiceWaitingConfirm.Other13FeeName,
                Other14FeeName = invoiceWaitingConfirm.Other14FeeName,
                Other15FeeName = invoiceWaitingConfirm.Other15FeeName,
                Other16FeeName = invoiceWaitingConfirm.Other16FeeName,
                Other17FeeName = invoiceWaitingConfirm.Other17FeeName,
                Other18FeeName = invoiceWaitingConfirm.Other18FeeName,                                                                                
                Comment = invoiceWaitingConfirm.Comment,
                IsActive = 1
            };

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

            var invoicePDFGeneratorService = new InvoicePDFGeneratorService(invoice, _log);
            invoicePDFGeneratorService.SavePDF();
             
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