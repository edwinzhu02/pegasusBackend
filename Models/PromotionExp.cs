using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Models
{
    public class PromotionExp
    {
       public IEnumerable<PromotionInvoice> PromotionInvoices { set; get; }
    }
    public class PromotionInvoice
    {
        public decimal Amt{set;get;}
        public IEnumerable<PromotionInvoiceItem> Item { set; get; }
    }
    public class PromotionInvoiceItem
    {
       public String Name { set; get; }
       public decimal? Amount { set; get; }       
    }
}