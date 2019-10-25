using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class CashBox
    {
        public int CashBoxId { get; set; }
        public DateTime? CashBoxDate { get; set; }
        public short? OrgId { get; set; }
        public decimal? YesterdayCash { get; set; }
        public decimal? TodayCash { get; set; }
        public decimal? InCash { get; set; }
        public decimal? OutCash { get; set; }
        public decimal? Cheque { get; set; }
        public decimal? BankDepoist { get; set; }
        public DateTime? CloseTime { get; set; }
        public decimal? Eftpos { get; set; }
        public short? StaffId { get; set; }

        public virtual Org Org { get; set; }
    }
}
