using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Course
    {
        public Course()
        {
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Lesson = new HashSet<Lesson>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
            TeacherCourse = new HashSet<TeacherCourse>();
        }

        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public byte? CourseType { get; set; }
        public byte? Level { get; set; }
        public short? Duration { get; set; }
        public decimal? Price { get; set; }
        public short? CourseCategoryId { get; set; }
        public byte? TeacherLevel { get; set; }

        public CourseCategory CourseCategory { get; set; }
        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public ICollection<TeacherCourse> TeacherCourse { get; set; }
    }
}
