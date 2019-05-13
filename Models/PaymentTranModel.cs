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
        public int LearnerId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public decimal Amount { get; set; }

        [JsonProperty(Required = Required.Always)]
        public short StaffId { get; set; }

        public int? InvoiceId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int OrgId { get; set; }

        public byte? PaymentType { get; set; }

        public ICollection<SoldTransaction> SoldTransaction { get; set; }

        public Payment Payment { get; set; }
        public Product Product { get; set; }

    }

}