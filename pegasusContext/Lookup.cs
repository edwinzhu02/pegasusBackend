using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Lookup
    {
        public int LookupId { get; set; }
        public string PropName { get; set; }
        public int? PropValue { get; set; }
        public short? LookupType { get; set; }
        public string Description { get; set; }
    }
}
