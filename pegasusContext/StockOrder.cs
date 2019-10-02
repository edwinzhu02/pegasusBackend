using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class StockOrder
    {
        public int OrderId { get; set; }
        public int? StockId { get; set; }
        public short? OrderType { get; set; }
        public int? ProductId { get; set; }
        public short? OrgId { get; set; }
        public int? Quantity { get; set; }
        public decimal? BuyingPrice { get; set; }
        public string ReceiptImg { get; set; }
        public short? StaffId { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Org Org { get; set; }
        public virtual Product Product { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual Stock Stock { get; set; }
    }
}
