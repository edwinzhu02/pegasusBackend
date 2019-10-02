using System;
using System.Collections.Generic;

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

        public virtual ICollection<TeacherLanguage> TeacherLanguage { get; set; }
    }
}
