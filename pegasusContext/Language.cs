using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class Language
    {
        public Language()
        {
            TeacherLanguage = new HashSet<TeacherLanguage>();
        }

        public byte LangId { get; set; }
        public string LangName { get; set; }
        
        
        [JsonIgnore]
        public ICollection<TeacherLanguage> TeacherLanguage { get; set; }
    }
}
