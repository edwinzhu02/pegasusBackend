using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Pegasus_backend.Models
{
    public class TeachersRegister
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public short Gender { get; set; }
        public DateTime Dob { get; set; }
        public string IRDNumber { get; set; }
        public short IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime IDExpireDate { get; set; }
        public List<string> Qualification { get; set; }
        public string PhoneNumber { get; set; }
        public string HomePhone { get; set; }
        public string Email { get; set; }
        public List<string> Language { get; set; }
        public List<List<string>> DayOfWeek { get; set; } 
        public IFormFile IdPhoto { get; set; }
        public IFormFile Photo { get; set; }
        
    }
}