using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Models
{
    public class TeacherCourseRegister
    {
        [Required(ErrorMessage = "CourseId is required")]
        public List<int> Courses { get; set; }
        [Required(ErrorMessage = "Teacher id is required")]
        public short TeacherId { get; set; }
        [Required(ErrorMessage = "Teacher wage rates is required")]
        public TeacherWageRates TeacherWageRates { get; set; }
    }
}