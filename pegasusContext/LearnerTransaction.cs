using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LearnerTransaction
    {
        public int TranId { get; set; }
        public int? LessonId { get; set; }
        public string CreatedAt { get; set; }
        public string Amount { get; set; }
        public int? LearnerId { get; set; }

        public Fund Learner { get; set; }
        public Lesson Lesson { get; set; }
    }
}
