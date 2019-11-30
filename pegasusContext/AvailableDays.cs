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

        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
    }
}
