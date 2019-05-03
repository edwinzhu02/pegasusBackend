using Pegasus_backend.pegasusContext;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Pegasus_backend.Models
{
    public class PaymentTranModel
    {
        public int? PaymentId { get; set; }
        public byte? PaymentMethod { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int? LearnerId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public decimal? Amount { get; set; }

        [JsonProperty(Required = Required.Always)]
        public short StaffId { get; set; }

        public int? InvoiceId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int OrgId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public IEnumerable<SoldTransaction> SoldTransaction { get; set; }
        public SoldTransaction Model { get; set; }

        public int? TranId { get; set; }

        //[JsonProperty(Required = Required.Always)]
        //public int? ProductId { get; set; }

        //public int? StockId { get; set; }
        //[JsonProperty(Required = Required.Always)]
        //public int? SoldQuantity { get; set; }
        //public decimal? DiscountAmount { get; set; }
        //public decimal? DiscountRate { get; set; }
        //public decimal? DiscountedAmount { get; set; }

        public Payment Payment { get; set; }
        public Product Product { get; set; }


        //public IEnumerable<> ProductTran { get; set; }
    }

}