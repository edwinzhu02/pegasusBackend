﻿using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Product
    {
        public Product()
        {
            ApplicationDetails = new HashSet<ApplicationDetails>();
            SoldTransaction = new HashSet<SoldTransaction>();
            Stock = new HashSet<Stock>();
            StockOrder = new HashSet<StockOrder>();
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public decimal? SellPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public short? ProdTypeId { get; set; }

        public virtual ProdType ProdType { get; set; }
        public virtual ICollection<ApplicationDetails> ApplicationDetails { get; set; }
        public virtual ICollection<SoldTransaction> SoldTransaction { get; set; }
        public virtual ICollection<Stock> Stock { get; set; }
        public virtual ICollection<StockOrder> StockOrder { get; set; }
    }
}
