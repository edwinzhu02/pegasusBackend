using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LessonRemain
    {
        public int LessonRemainId { get; set; }
        public int? Quantity { get; set; }
        public short? TermId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public int? LearnerId { get; set; }

        public One2oneCourseInstance CourseInstance { get; set; }
        public GroupCourseInstance GroupCourseInstance { get; set; }
        public Learner Learner { get; set; }
        public Term Term { get; set; }
    }
}
