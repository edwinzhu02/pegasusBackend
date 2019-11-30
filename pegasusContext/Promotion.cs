using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Promotion
    {
        public Promotion()
        {
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
        }

        public byte PromotionId { get; set; }
        public string PromotionName { get; set; }
        public string PromotionExp { get; set; }

        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
    }
}
