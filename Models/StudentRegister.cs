using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pegasus_backend.pegasusContext;

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
        public DateTime? dob { get; set; }
        public DateTime EnrollDate { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ContactNum { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(Required = Required.Always)]
        public byte LevelType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public byte LearnerLevel { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short OrgId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Address { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short IsUnder18 { get; set; }
        public string Note { get; set; }
        public string Comment { get; set; }
        public string Referrer { get; set; }
        public byte PaymentPeriod { get; set; }
        public List<Parent> Parent { get; set; }
        public List<LearnerGroupCourse> LearnerGroupCourse { get; set; }
        public List<OnetoOneCourseInstanceModel> One2oneCourseInstance { get; set; }
        public List<LearnerOthers> LearnerOthers { get; set; }
        
    }
}