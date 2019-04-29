using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public ICollection<ProdType> ProdType { get; set; }
    }
}
