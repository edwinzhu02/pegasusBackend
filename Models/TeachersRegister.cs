using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Pegasus_backend.Models
{
    public class TeachersRegister
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public short Gender { get; set; }
        [Required(ErrorMessage = "Dob is required")]
        public DateTime Dob { get; set; }
        [Required(ErrorMessage = "IDR number is required")]
        public string IRDNumber { get; set; }
        [Required(ErrorMessage = "Id type is required")]
        public short IDType { get; set; }
        [Required(ErrorMessage = "Id number is required")]
        public string IDNumber { get; set; }
        [Required(ErrorMessage = "ExpiryDate is required")]
        public DateTime? ExpiryDate { get; set; }
        public List<string> Qualification { get; set; }
        [Required(ErrorMessage = "Mobile phone number is required")]
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Language is required")]
        public List<string> Language { get; set; }
        [Required(ErrorMessage = "DayOfweek is required")]
        public List<List<string>> DayOfWeek { get; set; } 
        
    }
}