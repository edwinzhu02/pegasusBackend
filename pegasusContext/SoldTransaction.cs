using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class SoldTransaction
    {
        public int TranId { get; set; }
        public int? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ProductId { get; set; }
        public int? LearnerId { get; set; }
        public decimal? Balance { get; set; }
        public string Note { get; set; }
        public int? BeforeQuantity { get; set; }
        public int? AflterQuantity { get; set; }
        public int? StockId { get; set; }
        public int? SoldQuantity { get; set; }

        public Fund Learner { get; set; }
        public Learner LearnerNavigation { get; set; }
        public Product Product { get; set; }
        public Stock Stock { get; set; }
    }
}
