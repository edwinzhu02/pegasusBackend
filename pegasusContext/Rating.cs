using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Rating
    {
        public int RatingId { get; set; }
        public short? RateType { get; set; }
        public string Comment { get; set; }
        public DateTime? CreateAt { get; set; }
        public int? LearnerId { get; set; }
        public short? TeacherId { get; set; }
        public int? LessonId { get; set; }
        public short? RateStar { get; set; }

        public virtual Learner Learner { get; set; }
        public virtual Lesson Lesson { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
