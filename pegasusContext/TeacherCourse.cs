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

        public Course Course { get; set; }
        public Teacher Teacher { get; set; }
    }
}
