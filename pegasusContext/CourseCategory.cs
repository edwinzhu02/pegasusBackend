using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class CourseCategory
    {
        public CourseCategory()
        {
            Course = new HashSet<Course>();
        }

        public short CourseCategoryId { get; set; }
        public string CourseCategoryName { get; set; }

        public ICollection<Course> Course { get; set; }
    }
}
