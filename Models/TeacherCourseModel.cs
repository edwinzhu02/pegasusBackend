using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class TeacherCourseModel
    {
        [Required(ErrorMessage = "CourseCategories is required")]
        public List<TeacherCourseCategoryModel> CourseCategories { get; set; }
    }
    
}