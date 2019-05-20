using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class TeacherCourseRegister
    {
        [Required(ErrorMessage = "CourseCategories is required")]
        public List<TeacherCourseModel> TeacherCourses { get; set; }
        public short TeacherId { get; set; }
    }
    public class TeacherCourseModel
    {
        [Required(ErrorMessage = "CourseId is required")]
        public int CourseId { get; set; }
        [Required(ErrorMessage = "HoulyWage is required")]
        public decimal HourlyWage { get; set; }
    }
}