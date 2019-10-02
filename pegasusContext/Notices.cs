using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Notices
    {
        public int NoticeId { get; set; }
        public string Notice { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? IsCompleted { get; set; }
        public short? FromStaffId { get; set; }
        public short? ToStaffId { get; set; }
        public short? IsRead { get; set; }

        public virtual Staff FromStaff { get; set; }
        public virtual Staff ToStaff { get; set; }
    }
}
