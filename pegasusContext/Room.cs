using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Room
    {
        public Room()
        {
            Amendment = new HashSet<Amendment>();
            AvailableDays = new HashSet<AvailableDays>();
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Lesson = new HashSet<Lesson>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
        }

        public short RoomId { get; set; }
        public short? OrgId { get; set; }
        public string RoomName { get; set; }
        public short? IsActivate { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Org Org { get; set; }
        public virtual ICollection<Amendment> Amendment { get; set; }
        public virtual ICollection<AvailableDays> AvailableDays { get; set; }
        public virtual ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        public virtual ICollection<Lesson> Lesson { get; set; }
        public virtual ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
    }
}
