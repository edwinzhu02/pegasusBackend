using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class TeacherSearch
    {
        [Required(ErrorMessage = "FirstName is required")]
        public bool FirstName { get; set; }
        [Required(ErrorMessage = "LastName is required")]
        public bool LastName { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public bool Gender { get; set; }
        [Required(ErrorMessage = "MobilePhone is required")]
        public bool MobilePhone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public bool Email { get; set; }
    }
}