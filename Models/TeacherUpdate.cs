using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pegasus_backend.Models
{
    
    public class TeachersUpdate
    {
        [JsonProperty(Required = Required.Always)]
        public byte? Level { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short IsContract { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short IsLeft { get; set; }
        public byte InvoiceTemplate { get; set; }
        public string Ability { get; set; }
        public string Comment { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string FirstName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string LastName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short Gender { get; set; }
        [JsonProperty(Required = Required.Always)]
        public DateTime Dob { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string IRDNumber { get; set; }
        public short IDType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string IDNumber { get; set; }
        [JsonProperty(Required = Required.Always)]
        public DateTime? ExpiryDate { get; set; }
        public List<byte> Qualificatiion { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(Required = Required.Always)]
        public List<byte> Language { get; set; }
        [JsonProperty(Required = Required.Always)]
        public List<List<byte>> DayOfWeek { get; set; } 
        
    }
}