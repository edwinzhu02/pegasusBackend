using Pegasus_backend.pegasusContext;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.Models
{
    public class ProdCatModel
    {
        public short ProdCatId { get; set; }
        public string ProdCatName { get; set; }
        [JsonIgnore]
        public IEnumerable<ProdType> ProdType { get; set; }

    }
}