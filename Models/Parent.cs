
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Pegasus_backend.Models
{
    public class Parent
    {
        [JsonProperty(Required = Required.Always)]
        public string FirstName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string LastName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public byte Relationship { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ContactNum { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; }
    }
}