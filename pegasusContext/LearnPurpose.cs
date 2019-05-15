using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LearnPurpose
    {
<<<<<<< HEAD
        public short LearnPurposeId { get; set; }
        public string Purpose { get; set; }
=======
        public LearnPurpose()
        {
            Learner = new HashSet<Learner>();
        }

        public short LearnPurposeId { get; set; }
        public string Purpose { get; set; }

        public ICollection<Learner> Learner { get; set; }
>>>>>>> 7299a5171805a791a0ce0beb98746fe977efb047
    }
}
