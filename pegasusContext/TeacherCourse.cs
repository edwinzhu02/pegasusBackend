using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherCourse
    {
        public int? CourseId { get; set; }
        public short? TeacherId { get; set; }
        public int TeacherCourseId { get; set; }
        public decimal? HourlyWage { get; set; }

        public virtual Course Course { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
