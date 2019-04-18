using System;
using System.Collections.Generic;

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

        public ICollection<TeacherQualificatiion> TeacherQualificatiion { get; set; }
    }
}
