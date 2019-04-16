using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherTransaction
    {
        public int TranId { get; set; }
        public int? LessonId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? WageAmount { get; set; }

        public Lesson Lesson { get; set; }
    }
}
