using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class AwaitMakeUpLesson
    {
        public int AwaitId { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? SchduledAt { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int? MissedLessonId { get; set; }
        public int? NewLessonId { get; set; }
        public short? IsActive { get; set; }
        public int? LearnerId { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public byte? Remaining { get; set; }

        public One2oneCourseInstance CourseInstance { get; set; }
        public GroupCourseInstance GroupCourseInstance { get; set; }
        public Learner Learner { get; set; }
        public Lesson MissedLesson { get; set; }
        public Lesson NewLesson { get; set; }
    }
}
