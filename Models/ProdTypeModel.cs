using Pegasus_backend.pegasusContext;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.Models
{
    public class ProdTypeModel
    {
        public short ProdTypeId { get; set; }
        public string ProdTypeName { get; set; }
        public short? ProdCatId { get; set; }

        public ProdCat ProdCat { get; set; }
        [JsonIgnore]
        public IEnumerable<Product> Product { get; set; }

    }
}