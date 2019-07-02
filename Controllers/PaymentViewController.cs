using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Services;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentViewController:BasicController
    {
        private readonly DataServicePayment _service;
        private IMapper _mapper;

        public PaymentViewController(ablemusicContext ablemusicContext, ILogger<PaymentViewController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _service = new DataServicePayment(_ablemusicContext);
            _mapper = mapper;
            
        }

        
        
        [HttpGet("{id}")]
        public Result<IEnumerable<Payment>> GetStudentPayment(int id)
        {
            var result = _service.LookUpById(id);
            return result;
            
        }


        [HttpGet("paymentBetween/{beginDate}&{endDate}")]
        public Result<IEnumerable<Payment>> LookUpBetweenDates(DateTime beginDate, DateTime endDate)
        {
            var result = _service.LookUpByDate(beginDate,endDate);
            return result;
        }
        
        [HttpGet("paymentBetweenDesc/{beginDate}&{endDate}")]
        public Result<IEnumerable<Payment>> LookUpBetweenDatesDesc(DateTime beginDate, DateTime endDate)
        {
            var result = _service.LookUpByDateDesc(beginDate,endDate);
            return result;
        }
        
        [HttpGet("PaymentInAsceOrder")]
        public Result<IEnumerable<Payment>> LookUpAsce()
        {
            var result = _service.LookUpByOrderASCE();
            return result;
            
        }
        
        [HttpGet("PaymentInDescOrder")]
        public Result<IEnumerable<Payment>> LookUpDesc()
        {
            var result = _service.LookUpByOrderDESC();
            return result;
            
        }
        
        
    }
}