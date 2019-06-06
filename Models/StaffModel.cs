using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Models
{
    public class StaffModel
    {
        [JsonProperty(Required = Required.Always)]
        public string FirstName { get; set; }
        public string Visa { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string LastName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        [JsonProperty(Required = Required.Always)]
        public DateTime? Dob { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short? Gender { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string IrdNumber { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short? IdType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string IdNumber { get; set; }
        public string HomePhone { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string MobilePhone { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short RoleId { get; set; }
        public List<StaffOrg> StaffOrg { get; set; }
    }
}