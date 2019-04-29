using Pegasus_backend.pegasusContext;
using System.Collections.Generic;


namespace Pegasus_backend.Models
{
    public class ProdCatModel
    {
        public short ProdCatId { get; set; }
        public string ProdCatName { get; set; }

        public IEnumerable<ProdType> ProdType { get; set; }

    }
}