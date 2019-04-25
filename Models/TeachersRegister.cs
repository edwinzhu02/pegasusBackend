using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pegasus_backend.Models
{
    public class TeachersRegister
    {
        [JsonProperty(Required = Required.Always)]
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
        public List<byte> Qualificatiion { get; set; }
        [Required(ErrorMessage = "Mobile phone number is required")]
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Language is required")]
        public List<byte> Language { get; set; }
        [Required(ErrorMessage = "DayOfweek is required")]
        public List<List<byte>> DayOfWeek { get; set; } 
        
    }
}