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

        public One2oneCourseInstance CourseInstance { get; set; }
        public GroupCourseInstance GroupCourseInstance { get; set; }
        public ICollection<Amendment> Amendment { get; set; }
    }
}
