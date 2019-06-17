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
                payments = _context.Payment.Where(p => p.LearnerId == studentId)
                    .Include(t => t.SoldTransaction)
                    .ThenInclude(t => t.Product)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.CourseInstance)
                    .ThenInclude(i => i.Course)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.GroupCourseInstance)
                    .ThenInclude(i => i.Course);

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
            IEnumerable<Payment> payments;
            try
            {
                payments = _context.Payment.Where(d => Between(d.CreatedAt, begin, end))
                    .Include(t => t.SoldTransaction)
                    .ThenInclude(t => t.Product)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.CourseInstance)
                    .ThenInclude(i => i.Course)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.GroupCourseInstance)
                    .ThenInclude(i => i.Course);

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
                    .Include(t => t.SoldTransaction)
                    .ThenInclude(t => t.Product)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.CourseInstance)
                    .ThenInclude(i => i.Course)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.GroupCourseInstance)
                    .ThenInclude(i => i.Course)
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
                payments = _context.Payment
                    .Include(t => t.SoldTransaction)
                    .ThenInclude(t => t.Product)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.CourseInstance)
                    .ThenInclude(i => i.Course)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.GroupCourseInstance)
                    .ThenInclude(i => i.Course)
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
                payments = _context.Payment.OrderByDescending(d=>d.CreatedAt)
                    .Include(t => t.SoldTransaction)
                    .ThenInclude(t => t.Product)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.CourseInstance)
                    .ThenInclude(i => i.Course)
                    .Include(i => i.Invoice)
                    .ThenInclude(i => i.GroupCourseInstance)
                    .ThenInclude(i => i.Course);
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