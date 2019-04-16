using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ProdCat
    {
        public ProdCat()
        {
            Product = new HashSet<Product>();
        }

        public short ProdCatId { get; set; }
        public string ProdCatName { get; set; }

        public ICollection<Product> Product { get; set; }
    }
}
