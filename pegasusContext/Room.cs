using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Room
    {
        public Room()
        {
            Amendment = new HashSet<Amendment>();
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Lesson = new HashSet<Lesson>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
        }

        public short RoomId { get; set; }
        public short? OrgId { get; set; }
        public string RoomName { get; set; }
        public short? IsActivate { get; set; }
        public DateTime? CreatedAt { get; set; }

        public Org Org { get; set; }
        public ICollection<Amendment> Amendment { get; set; }
        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
    }
}
