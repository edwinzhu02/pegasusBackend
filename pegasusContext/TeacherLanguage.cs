using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherLanguage
    {
        public short? TeacherId { get; set; }
        public short TeacherLangId { get; set; }
        public byte? LangId { get; set; }

        public virtual Language Lang { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
