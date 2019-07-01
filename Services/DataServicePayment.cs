using System.Collections.Generic;
using System;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Pegasus_backend.Services
{
    public class DataServicePayment
    {
        private readonly ablemusicContext _context;

        public DataServicePayment(ablemusicContext context)
        {
            _context = context;
        }

        public Result<IEnumerable<Payment>> LookUpById(int studentId)
        {
            Result<IEnumerable<Payment>> result = new Result<IEnumerable<Payment>>();
            IEnumerable<Payment> payments;
            try
            {
                payments = _context.Payment.Where(p=>p.LearnerId == studentId)
                    .GroupJoin(_context.Invoice,
                        payment => payment.InvoiceId,
                        invoice => invoice.InvoiceId,
                        (payment, invoice) => new Payment
                        {
                            PaymentId = payment.PaymentId,
                            PaymentMethod = payment.PaymentMethod,
                            LearnerId = payment.LearnerId,
                            Amount = payment.Amount,
                            CreatedAt = payment.CreatedAt,
                            StaffId = payment.StaffId,
                            InvoiceId = payment.InvoiceId,
                            Invoice = invoice.FirstOrDefault(),
                            BeforeBalance = payment.BeforeBalance,
                            AfterBalance = payment.AfterBalance,
                            PaymentType = payment.PaymentType,
                            IsConfirmed = payment.IsConfirmed,
                            Comment = payment.Comment,
                            SoldTransaction = payment.SoldTransaction

                        }).GroupJoin(_context.SoldTransaction,
                        p => p.PaymentId,
                        s => s.PaymentId,
                        (p, s) => new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = s.ToList()

                        }
                    ).GroupJoin(_context.Staff,
                        p=>p.StaffId,
                        s=>s.StaffId,
                        (p,s)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.PaymentMethod !=1?s.FirstOrDefault():null
                        }).GroupJoin(_context.Learner,
                        p=>p.LearnerId,
                        l=>l.LearnerId,
                        (p,l)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.Staff,
                            Learner = l.FirstOrDefault()
                        });

            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Data = payments;
            return result;
        }

        public static bool Between(DateTime? input, DateTime? date1, DateTime? date2)
        {
            var end = DateTime.Today;
            
            if (date2 != null)
            {
                end = date2.GetValueOrDefault();}

            end = end.AddDays(1);
            return input >= date1 && input < end;
        }

        public Result<IEnumerable<Payment>> LookUpByDate(DateTime begin, DateTime end)
        {
            Result<IEnumerable<Payment>> result = new Result<IEnumerable<Payment>>();
            if(end==null){end=DateTime.Today;}
            //IEnumerable<Payment> payments;
            IEnumerable<Payment> payments;
            try
            {
                payments  = _context.Payment.Where(d => Between(d.CreatedAt, begin, end))
                    .GroupJoin(_context.Invoice,
                    payment=>payment.InvoiceId,
                    invoice => invoice.InvoiceId,
                    (payment,invoice)=> new Payment
                    {
                        PaymentId = payment.PaymentId,
                        PaymentMethod = payment.PaymentMethod,
                        LearnerId = payment.LearnerId,
                        Amount = payment.Amount,
                        CreatedAt = payment.CreatedAt,
                        StaffId = payment.StaffId,
                        InvoiceId = payment.InvoiceId,
                        Invoice = invoice.FirstOrDefault(),
                        BeforeBalance = payment.BeforeBalance,
                        AfterBalance = payment.AfterBalance,
                        PaymentType = payment.PaymentType,
                        IsConfirmed = payment.IsConfirmed,
                        Comment = payment.Comment,
                        SoldTransaction = payment.SoldTransaction
                        
                    }).GroupJoin(_context.SoldTransaction,
                        p=>p.PaymentId,
                        s=>s.PaymentId,
                        (p,s)=> new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = s.ToList()
                            
                        }
                        ).GroupJoin(_context.Staff,
                        p=>p.StaffId,
                        s=>s.StaffId,
                        (p,s)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.PaymentMethod!=1? s.FirstOrDefault():null
                        }).GroupJoin(_context.Learner,
                        p=>p.LearnerId,
                        l=>l.LearnerId,
                        (p,l)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.Staff,
                            Learner = l.FirstOrDefault()
                        });

            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Data = payments;
            return result;
        }
        
        public Result<IEnumerable<Payment>> LookUpByDateDesc(DateTime begin, DateTime end)
        {
            Result<IEnumerable<Payment>> result = new Result<IEnumerable<Payment>>();
            if(end==null){end=DateTime.Today;}
            IEnumerable<Payment> payments;
            try
            {
                payments = _context.Payment.Where(d => Between(d.CreatedAt, begin, end))
                    .GroupJoin(_context.Invoice,
                    payment=>payment.InvoiceId,
                    invoice => invoice.InvoiceId,
                    (payment,invoice)=> new Payment
                    {
                        PaymentId = payment.PaymentId,
                        PaymentMethod = payment.PaymentMethod,
                        LearnerId = payment.LearnerId,
                        Amount = payment.Amount,
                        CreatedAt = payment.CreatedAt,
                        StaffId = payment.StaffId,
                        InvoiceId = payment.InvoiceId,
                        Invoice = invoice.FirstOrDefault(),
                        BeforeBalance = payment.BeforeBalance,
                        AfterBalance = payment.AfterBalance,
                        PaymentType = payment.PaymentType,
                        IsConfirmed = payment.IsConfirmed,
                        Comment = payment.Comment,
                        SoldTransaction = payment.SoldTransaction
                        
                    }).GroupJoin(_context.SoldTransaction,
                        p=>p.PaymentId,
                        s=>s.PaymentId,
                        (p,s)=> new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = s.ToList()
                            
                        }
                        ).GroupJoin(_context.Staff,
                        p=>p.StaffId,
                        s=>s.StaffId,
                        (p,s)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.PaymentMethod!=1? s.FirstOrDefault():null
                        }).GroupJoin(_context.Learner,
                        p=>p.LearnerId,
                        l=>l.LearnerId,
                        (p,l)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.Staff,
                            Learner = l.FirstOrDefault()
                        })
                    
                    .OrderByDescending(d=>d.CreatedAt);
                
                    
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Data = payments;
            return result;
        }

        public Result<IEnumerable<Payment>> LookUpByOrderASCE()
        {
            Result<IEnumerable<Payment>> result = new Result<IEnumerable<Payment>>();
            IEnumerable<Payment> payments;
            try
            {
                payments  = _context.Payment
                    .GroupJoin(_context.Invoice,
                    payment=>payment.InvoiceId,
                    invoice => invoice.InvoiceId,
                    (payment,invoice)=> new Payment
                    {
                        PaymentId = payment.PaymentId,
                        PaymentMethod = payment.PaymentMethod,
                        LearnerId = payment.LearnerId,
                        Amount = payment.Amount,
                        CreatedAt = payment.CreatedAt,
                        StaffId = payment.StaffId,
                        InvoiceId = payment.InvoiceId,
                        Invoice = invoice.FirstOrDefault(),
                        BeforeBalance = payment.BeforeBalance,
                        AfterBalance = payment.AfterBalance,
                        PaymentType = payment.PaymentType,
                        IsConfirmed = payment.IsConfirmed,
                        Comment = payment.Comment,
                        SoldTransaction = payment.SoldTransaction
                        
                    }).GroupJoin(_context.SoldTransaction,
                        p=>p.PaymentId,
                        s=>s.PaymentId,
                        (p,s)=> new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = s.ToList()
                            
                        }
                        ).GroupJoin(_context.Staff,
                        p=>p.StaffId,
                        s=>s.StaffId,
                        (p,s)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.PaymentMethod!=1? s.FirstOrDefault():null
                        }).GroupJoin(_context.Learner,
                        p=>p.LearnerId,
                        l=>l.LearnerId,
                        (p,l)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.Staff,
                            Learner = l.FirstOrDefault()
                        })
                    .OrderBy(d=>d.CreatedAt);
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Data = payments;
            return result;
            
        }

        public Result<IEnumerable<Payment>> LookUpByOrderDESC()
        {
            Result<IEnumerable<Payment>> result = new Result<IEnumerable<Payment>>();
            IEnumerable<Payment> payments;
            try
            {
                payments  = _context.Payment
                    .GroupJoin(_context.Invoice,
                    payment=>payment.InvoiceId,
                    invoice => invoice.InvoiceId,
                    (payment,invoice)=> new Payment
                    {
                        PaymentId = payment.PaymentId,
                        PaymentMethod = payment.PaymentMethod,
                        LearnerId = payment.LearnerId,
                        Amount = payment.Amount,
                        CreatedAt = payment.CreatedAt,
                        StaffId = payment.StaffId,
                        InvoiceId = payment.InvoiceId,
                        Invoice = invoice.FirstOrDefault(),
                        BeforeBalance = payment.BeforeBalance,
                        AfterBalance = payment.AfterBalance,
                        PaymentType = payment.PaymentType,
                        IsConfirmed = payment.IsConfirmed,
                        Comment = payment.Comment,
                        SoldTransaction = payment.SoldTransaction
                        
                    }).GroupJoin(_context.SoldTransaction,
                        p=>p.PaymentId,
                        s=>s.PaymentId,
                        (p,s)=> new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = s.ToList()
                            
                        }
                        ).GroupJoin(_context.Staff,
                        p=>p.StaffId,
                        s=>s.StaffId,
                        (p,s)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.PaymentMethod!=1? s.FirstOrDefault():null
                        }).GroupJoin(_context.Learner,
                        p=>p.LearnerId,
                        l=>l.LearnerId,
                        (p,l)=>new Payment
                        {
                            PaymentId = p.PaymentId,
                            PaymentMethod = p.PaymentMethod,
                            LearnerId = p.LearnerId,
                            Amount = p.Amount,
                            CreatedAt = p.CreatedAt,
                            StaffId = p.StaffId,
                            InvoiceId = p.InvoiceId,
                            Invoice = p.Invoice,
                            BeforeBalance = p.BeforeBalance,
                            AfterBalance = p.AfterBalance,
                            PaymentType = p.PaymentType,
                            IsConfirmed = p.IsConfirmed,
                            Comment = p.Comment,
                            SoldTransaction = p.SoldTransaction,
                            Staff = p.Staff,
                            Learner = l.FirstOrDefault()
                        });
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Data = payments;
            return result;
        }

    }
}