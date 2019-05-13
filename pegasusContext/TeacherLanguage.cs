using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherLanguage
    {
        public short? TeacherId { get; set; }
        public short TeacherLangId { get; set; }
        public byte? LangId { get; set; }

        public Language Lang { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }
    }
}
