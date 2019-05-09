using Pegasus_backend.pegasusContext;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "ProductName is Required.")]
        public string ProductName { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Please enter a {0} value bigger than 0.")]
        [Required(ErrorMessage = "SellPrice is Required.")]
        public decimal? SellPrice { get; set; }
        public decimal? WholesalePrice { get; set; }

        [Required(ErrorMessage ="ProdType is Required.")]
        public short? ProdTypeId { get; set; }

        public ProdType ProdType { get; set; }
        public IEnumerable<SoldTransaction> SoldTransaction { get; set; }
        public IEnumerable<Stock> Stock { get; set; }
        public IEnumerable<StockOrder> StockOrder { get; set; }

    }
}