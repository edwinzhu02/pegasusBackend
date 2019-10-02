using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherQualificatiion
    {
        public short? TeacherId { get; set; }
        public short TeacherQualiId { get; set; }
        public string Description { get; set; }
        public byte? QualiId { get; set; }

        public virtual Qualification Quali { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
