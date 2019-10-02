using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class RoleAccess
    {
        public short? RoleId { get; set; }
        public int? PageId { get; set; }
        public short RoleAccessId { get; set; }
        public short? IsMobile { get; set; }

        public virtual Page Page { get; set; }
        public virtual Role Role { get; set; }
    }
}
