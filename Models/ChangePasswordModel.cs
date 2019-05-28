using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "userName is required")]
        public string userName { get; set; }
        [Required(ErrorMessage = "oldPassword is required")]
        public string oldPassword { get; set; }
        [Required(ErrorMessage = "newPassword is required")]
        public string newPassword { get; set; }
    }
}