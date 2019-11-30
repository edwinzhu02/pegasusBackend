using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LoginLog
    {
        public short? UserId { get; set; }
        public int LoginLogId { get; set; }
        public int? LogType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? OrgId { get; set; }

        public Org Org { get; set; }
        public User User { get; set; }
    }
}
