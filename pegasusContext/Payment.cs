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
        public short? OrgId { get; set; }

        public virtual Invoice Invoice { get; set; }
        public virtual Learner Learner { get; set; }
        public virtual Org Org { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual ICollection<SoldTransaction> SoldTransaction { get; set; }
    }
}
