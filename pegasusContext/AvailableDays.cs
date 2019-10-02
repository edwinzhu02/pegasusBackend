using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class AvailableDays
    {
        public short? TeacherId { get; set; }
        public int AvailableDaysId { get; set; }
        public byte? DayOfWeek { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? OrgId { get; set; }
        public short? RoomId { get; set; }

        public virtual Org Org { get; set; }
        public virtual Room Room { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
