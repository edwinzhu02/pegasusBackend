using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Pegasus_backend.Models
{
    public class StudentRegister
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public short Gender { get; set; }
        public DateTime dob { get; set; }
        public DateTime DateOfEnrollment { get; set; }
        public string ContactPhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public IFormFile ABRSM { get; set; }
        public IFormFile image { get; set; }
        public string GuardianFirstName { get; set; }
        public string GuardianLastName { get; set; }
        public byte GuardianRelationship { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianEmail { get; set; }
        /*public List<GroupCourse> GroupCourses { get; set; }
        public List<OnetoOneCourse> OnetoOneCourses { get; set; }*/
        
    }
}