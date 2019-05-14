using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LearnPurpose
    {
        public LearnPurpose()
        {
            Learner = new HashSet<Learner>();
        }

        public short LearnPurposeId { get; set; }
        public string Purpose { get; set; }

        public ICollection<Learner> Learner { get; set; }
    }
}
