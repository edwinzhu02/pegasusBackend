using Pegasus_backend.pegasusContext;
using System.Collections.Generic;

namespace Pegasus_backend.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public decimal? SellPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public short? ProdTypeId { get; set; }

        public ProdType ProdType { get; set; }
        public IEnumerable<SoldTransaction> SoldTransaction { get; set; }
        public IEnumerable<Stock> Stock { get; set; }
        public IEnumerable<StockOrder> StockOrder { get; set; }

    }
}