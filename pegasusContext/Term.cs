using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Term
    {
        public Term()
        {
            Invoice = new HashSet<Invoice>();
            InvoiceWaitingConfirm = new HashSet<InvoiceWaitingConfirm>();
            LessonRemain = new HashSet<LessonRemain>();
        }

        public short TermId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? WeekQuantity { get; set; }
        public byte? TermType { get; set; }
<<<<<<< HEAD
        
        [JsonIgnore]
=======
>>>>>>> dc29658ef159cfb3115d26376761ff66c5f0de77

        public ICollection<Invoice> Invoice { get; set; }
        public ICollection<InvoiceWaitingConfirm> InvoiceWaitingConfirm { get; set; }
        public ICollection<LessonRemain> LessonRemain { get; set; }
    }
}
