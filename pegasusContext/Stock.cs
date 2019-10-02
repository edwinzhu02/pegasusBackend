using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Stock
    {
        public Stock()
        {
            SoldTransaction = new HashSet<SoldTransaction>();
            StockOrder = new HashSet<StockOrder>();
        }

        public int StockId { get; set; }
        public short? OrgId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }

        public virtual Org Org { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<SoldTransaction> SoldTransaction { get; set; }
        public virtual ICollection<StockOrder> StockOrder { get; set; }
    }
}
