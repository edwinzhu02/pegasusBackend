using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class AvailableDays
    {
        public short? TeacherId { get; set; }
        public int AvailableDaysId { get; set; }
        public byte? DayOfWeek { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? OrgId { get; set; }
        [JsonIgnore]

        public Org Org { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }
    }
}
