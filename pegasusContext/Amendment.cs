using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Amendment
    {
        public int? CourseInstanceId { get; set; }
        public int AmendmentId { get; set; }
        public short? OrgId { get; set; }
        public short? DayOfWeek { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Reason { get; set; }
        public short? IsTemporary { get; set; }
        public byte? AmendType { get; set; }
        public int? CourseScheduleId { get; set; }
        public short? TeacherId { get; set; }

        public One2oneCourseInstance CourseInstance { get; set; }
        public CourseSchedule CourseSchedule { get; set; }
        public Learner Learner { get; set; }
        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
    }
}
