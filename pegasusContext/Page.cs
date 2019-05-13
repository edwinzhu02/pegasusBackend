using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Page
    {
        public Page()
        {
            RoleAccess = new HashSet<RoleAccess>();
        }

        public int PageId { get; set; }
        public string PageName { get; set; }
        public short? PageGroupId { get; set; }
        public short? DisplayOrder { get; set; }
        public string Para { get; set; }
        public short? ParaFlag { get; set; }
        public short? IsActivate { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }

        public PageGroup PageGroup { get; set; }
        public ICollection<RoleAccess> RoleAccess { get; set; }
    }
}
