using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ParentAppForm
    {
        public int ParentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNum { get; set; }
        public byte? Relationship { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ParentFormId { get; set; }
        public int? AppFormId { get; set; }

        public LearnerAppForm AppForm { get; set; }
    }
}
