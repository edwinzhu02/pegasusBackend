using System;
using System.Collections.Generic;

namespace Pegasus_backend.ablemusicContext
{
    public partial class LoginLog
    {
        public short? UserId { get; set; }
        public int LoginLogId { get; set; }
        public int? LogType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Ipaddr { get; set; }
        public string Mac { get; set; }

        public User User { get; set; }
    }
}
