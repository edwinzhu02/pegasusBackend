using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Payment
    {
        public Payment()
        {
            SoldTransaction = new HashSet<SoldTransaction>();
        }

        public int PaymentId { get; set; }
        public byte? PaymentMethod { get; set; }
        public int? LearnerId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? StaffId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal? BeforeBalance { get; set; }
        public decimal? AfterBalance { get; set; }
        public byte? PaymentType { get; set; }
        public short? IsConfirmed { get; set; }
        public string Comment { get; set; }

        public Invoice Invoice { get; set; }
        public Learner Learner { get; set; }
        public Staff Staff { get; set; }
        public ICollection<SoldTransaction> SoldTransaction { get; set; }
    }
}
