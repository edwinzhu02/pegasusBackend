using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ProdType
    {
        public ProdType()
        {
            Product = new HashSet<Product>();
        }

        public short ProdTypeId { get; set; }
        public string ProdTypeName { get; set; }
        public short? ProdCatId { get; set; }

        public ProdCat ProdCat { get; set; }
        public ICollection<Product> Product { get; set; }
    }
}
