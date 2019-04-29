using Pegasus_backend.pegasusContext;
using System.Collections.Generic;


namespace Pegasus_backend.Models
{
    public class ProdTypeModel
    {
        public short ProdTypeId { get; set; }
        public string ProdTypeName { get; set; }
        public short? ProdCatId { get; set; }

        public ProdCat ProdCat { get; set; }

        public IEnumerable<Product> Product { get; set; }

    }
}