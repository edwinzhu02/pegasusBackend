using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LearnerGroupCourse
    {
        public int LearnerGroupCourseId { get; set; }
        public int? LearnerId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? IsActivate { get; set; }
        public string Comment { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public GroupCourseInstance GroupCourseInstance { get; set; }
        public Learner Learner { get; set; }
    }
}
