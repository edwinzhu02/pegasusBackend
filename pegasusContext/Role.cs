using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Role
    {
        public Role()
        {
            RoleAccess = new HashSet<RoleAccess>();
            User = new HashSet<User>();
        }

        public short RoleId { get; set; }
        public string RoleName { get; set; }

        public virtual ICollection<RoleAccess> RoleAccess { get; set; }
        public virtual ICollection<User> User { get; set; }
    }
}
