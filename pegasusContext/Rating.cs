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

        public Learner Learner { get; set; }
        public Teacher Teacher { get; set; }
    }
}
