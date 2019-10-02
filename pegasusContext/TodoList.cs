using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TodoList
    {
        public int ListId { get; set; }
        public string ListName { get; set; }
        public string ListContent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public short? ProcessFlag { get; set; }
        public short? UserId { get; set; }
        public DateTime? TodoDate { get; set; }
        public int? LessonId { get; set; }
        public int? LearnerId { get; set; }
        public short? TeacherId { get; set; }

        public virtual Learner Learner { get; set; }
        public virtual Lesson Lesson { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual User User { get; set; }
    }
}
