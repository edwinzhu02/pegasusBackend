﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class Course
    {
        public Course()
        {
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
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

        public CourseCategory CourseCategory { get; set; }
        [JsonIgnore]
        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        [JsonIgnore]
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public ICollection<TeacherCourse> TeacherCourse { get; set; }
    }
}
