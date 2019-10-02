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

        public virtual AwaitMakeUpLesson Await { get; set; }
        public virtual Lesson Lesson { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
