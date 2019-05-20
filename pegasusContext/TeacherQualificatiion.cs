using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherQualificatiion
    {
        public short? TeacherId { get; set; }
        public short TeacherQualiId { get; set; }
        public string Description { get; set; }
        public byte? QualiId { get; set; }

        public Qualification Quali { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }
    }
}
