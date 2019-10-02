using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class StaffOrg
    {
        public short? StaffId { get; set; }
        public short? OrgId { get; set; }
        public short StaffOrgId { get; set; }
        public short? IsActivate { get; set; }

        public virtual Org Org { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
