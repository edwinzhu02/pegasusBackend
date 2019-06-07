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

        public Result<IEnumerable<Payment>> GetPayment(int studentId)
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
        

    }
}