using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class RemindLog
    {
        public int RemindId { get; set; }
        public int? LearnerId { get; set; }
        public string Email { get; set; }
        public short? RemindType { get; set; }
        public string RemindContent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? TeacherId { get; set; }
        public short? IsLearner { get; set; }
        public short? ProcessFlag { get; set; }
        public DateTime? EmailAt { get; set; }
        public string RemindTitle { get; set; }
        public short? ReceivedFlag { get; set; }
        public int? LessonId { get; set; }
        public DateTime? ScheduledDate { get; set; }

        public virtual Learner Learner { get; set; }
        public virtual Lesson Lesson { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
