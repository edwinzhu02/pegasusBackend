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
        public TeacherWageRatesModel TeacherWageRates { get; set; }
    }


    public class TeacherWageRatesModel
    {
        [Required(ErrorMessage = "PianoRates is required")]
        public decimal? PianoRates { get; set; }
        [Required(ErrorMessage = "OthersRates is required")]
        public decimal? OthersRates { get; set; }
        [Required(ErrorMessage = "GroupRates is required")]
        public decimal? GroupRates { get; set; }
        [Required(ErrorMessage = "TheoryRates is required")]
        public decimal? TheoryRates { get; set; }
    }
}