using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ApplicationDetails
    {
        public int? ApplicationId { get; set; }
        public int DetaillsId { get; set; }
        public int ProductId { get; set; }
        public int? AppliedQty { get; set; }
        public int? DeliveredQty { get; set; }
        public int? ReceivedQty { get; set; }

        public virtual StockApplication Application { get; set; }
        public virtual Product Product { get; set; }
    }
}
