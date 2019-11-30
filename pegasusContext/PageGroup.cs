using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class PageGroup
    {
        public PageGroup()
        {
            Page = new HashSet<Page>();
        }

        public short PageGroupId { get; set; }
        public string PageGroupName { get; set; }
        public int? DisplayOrder { get; set; }
        public string Icon { get; set; }

        public ICollection<Page> Page { get; set; }
    }
}
