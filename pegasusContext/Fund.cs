using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Fund
    {
        public Fund()
        {
            LearnerTransaction = new HashSet<LearnerTransaction>();
            SoldTransaction = new HashSet<SoldTransaction>();
        }

        public decimal? Balance { get; set; }
        public int LearnerId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<LearnerTransaction> LearnerTransaction { get; set; }
        public ICollection<SoldTransaction> SoldTransaction { get; set; }
    }
}
