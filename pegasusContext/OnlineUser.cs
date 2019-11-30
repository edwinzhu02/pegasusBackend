using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class OnlineUser
    {
        public short UserId { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string Token { get; set; }

        public User User { get; set; }
    }
}
