using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Pegasus_backend.Models
{
    public class StudentRegister
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public short Gender { get; set; }
        [Required(ErrorMessage = "DOB is required")]
        public DateTime dob { get; set; }
        public DateTime DateOfEnrollment { get; set; }
        public string ContactPhone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        public IFormFile ABRSM { get; set; }
        public IFormFile image { get; set; }
        [Required(ErrorMessage = "Parent First Name is required")]
        public string GuardianFirstName { get; set; }
        [Required(ErrorMessage = "Parent Last Name is required")]
        public string GuardianLastName { get; set; }
        [Required(ErrorMessage = "Relationship is required")]
        public byte GuardianRelationship { get; set; }
        [Required(ErrorMessage = "Parent Phone is required")]
        public string GuardianPhone { get; set; }
        [Required(ErrorMessage = "Parent email is required")]
        public string GuardianEmail { get; set; }
        public List<int> GroupCourses { get; set; }
        //public List<OnetoOneCourse> OnetoOneCourses { get; set; }
        
    }
}