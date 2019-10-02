using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class CourseSchedule
    {
        public CourseSchedule()
        {
            Amendment = new HashSet<Amendment>();
        }

        public int CourseScheduleId { get; set; }
        public byte? DayOfWeek { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public virtual One2oneCourseInstance CourseInstance { get; set; }
        public virtual GroupCourseInstance GroupCourseInstance { get; set; }
        public virtual ICollection<Amendment> Amendment { get; set; }
    }
}
