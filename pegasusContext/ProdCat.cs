using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ProdCat
    {
        public ProdCat()
        {
            ProdType = new HashSet<ProdType>();
        }

        public short ProdCatId { get; set; }
        public string ProdCatName { get; set; }

        public ICollection<ProdType> ProdType { get; set; }
    }
}
