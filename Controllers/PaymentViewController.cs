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

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentViewController:ControllerBase
    {
        private readonly DataServicePayment _service;
        private IMapper _mapper;

        public PaymentViewController(ablemusicContext pegasusContext, IMapper mapper)
        {
            _service = new DataServicePayment(pegasusContext);
            _mapper = mapper;
            
        }

        [HttpGet("{id}")]
        public Result<IEnumerable<Payment>> GetStudentPayment(int id)
        {
            var result = _service.LookUpById(id);
            return result;
            
        }


        [HttpGet("btn/{begin}&{end}")]
        public Result<IEnumerable<Payment>> LookUpBetweenDates(DateTime begin, DateTime end)
        {
            var result = _service.LookUpByDate(begin,end);
            return result;
        }
        
        [HttpGet("btndesc/{begin}&{end}")]
        public Result<IEnumerable<Payment>> LookUpBetweenDatesDesc(DateTime begin, DateTime end)
        {
            var result = _service.LookUpByDateDesc(begin,end);
            return result;
        }
        
        [HttpGet("asce")]
        public Result<IEnumerable<Payment>> LookUpAsce()
        {
            var result = _service.LookUpByOrderASCE();
            return result;
            
        }
        
        [HttpGet("desc")]
        public Result<IEnumerable<Payment>> LookUpDesc()
        {
            var result = _service.LookUpByOrderDESC();
            return result;
            
        }
        
        
    }
}