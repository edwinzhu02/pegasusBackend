using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class SplittedLesson
    {
        public int SplittedId { get; set; }
        public int? AwaitId { get; set; }
        public int? LessonId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? StaffId { get; set; }
        public short? IsAfter { get; set; }

        public AwaitMakeUpLesson Await { get; set; }
        public Lesson Lesson { get; set; }
        public Staff Staff { get; set; }
    }
}
