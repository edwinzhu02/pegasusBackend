using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LessonMessage
    {
        public int MessageId { get; set; }
        public int? LessonId { get; set; }
        public short? TeacherId { get; set; }
        public int? LearnerId { get; set; }
        public short? StaffId { get; set; }
        public string Message { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? IsLearnerSent { get; set; }
        public string LearnerEmail { get; set; }
        public byte? IsTeacherSent { get; set; }
        public string TeacherEmail { get; set; }

        public Learner Learner { get; set; }
        public Lesson Lesson { get; set; }
        public Staff Staff { get; set; }
        public Teacher Teacher { get; set; }
    }
}
