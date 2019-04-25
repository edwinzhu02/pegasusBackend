using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pegasus_backend.Models
{
    public class StudentRegister
    {
        [JsonProperty(Required = Required.Always)]
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string LastName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short Gender { get; set; }
        [JsonProperty(Required = Required.Always)]
        public DateTime dob { get; set; }
        public DateTime DateOfEnrollment { get; set; }
        public string ContactPhone { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Address { get; set; }
        /*[Required(ErrorMessage = "Parent details is required")]*/
        public List<Parent> Parent { get; set; }
        public IFormFile ABRSM { get; set; }
        public IFormFile image { get; set; }
        
        //public List<int> GroupCourses { get; set; }
        //public List<OnetoOneCourse> OnetoOneCourses { get; set; }
        
    }
}