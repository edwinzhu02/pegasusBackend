using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Parent
    {
        public int ParentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNum { get; set; }
        public byte? Relationship { get; set; }
        public int? LearnerId { get; set; }
        public DateTime? CreatedAt { get; set; }

        public Learner Learner { get; set; }
    }
}
