using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class Qualification
    {
        public Qualification()
        {
            TeacherQualificatiion = new HashSet<TeacherQualificatiion>();
        }

        public byte QualiId { get; set; }
        public string QualiName { get; set; }

        [JsonIgnore]
        public ICollection<TeacherQualificatiion> TeacherQualificatiion { get; set; }
    }
}
