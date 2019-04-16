using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class UsrAndPass
    {
        [Required(ErrorMessage = "Username is required")]
        public string username { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }
    }
}