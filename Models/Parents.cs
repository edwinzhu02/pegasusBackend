
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
namespace Pegasus_backend.Models
{
    public class Parents
    {
        [Required(ErrorMessage = "Parent First Name is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Parent Last Name is required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Relationship is required")]
        public byte Relationship { get; set; }
        [Required(ErrorMessage = "Parent Phone is required")]
        public string ContactNum { get; set; }
        [Required(ErrorMessage = "Parent email is required")]
        public string Email { get; set; }
    }
}