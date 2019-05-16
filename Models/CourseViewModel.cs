using System.ComponentModel.DataAnnotations;


namespace Pegasus_backend.Models
{
    public class CourseViewModel
    {
        public int? CourseId { get; set; }
        [Required(ErrorMessage = "Course name is required")]
        public string CourseName { get; set; }
        [Required(ErrorMessage = "Course type is required")]
        [Range(1, 2, ErrorMessage ="Course type only allow 1 and 2")]
        public byte? CourseType { get; set; }
        [Required(ErrorMessage = "Level is required")]
        public byte? Level { get; set; }
        [Required(ErrorMessage = "Duration is required")]
        public short? Duration { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public decimal? Price { get; set; }
        [Required(ErrorMessage = "Course category id is required")]
        public short? CourseCategoryId { get; set; }
        [Required(ErrorMessage = "Teacher level is required")]
        public byte? TeacherLevel { get; set; }
    }
}
