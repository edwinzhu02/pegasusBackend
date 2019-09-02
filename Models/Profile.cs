using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pegasus_backend.pegasusContext;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class ProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "Contact is required")]        
        public string ContactNum { get; set; }
        [Required(ErrorMessage = "Email is required")]   
        public string Email { get; set; }
    }
}
