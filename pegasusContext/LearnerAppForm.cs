using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class LearnerAppForm
    {
        public LearnerAppForm()
        {
            ParentAppForm = new HashSet<ParentAppForm>();
        }

        public int LearnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? EnrollDate { get; set; }
        public string ContactNum { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public short? IsUnder18 { get; set; }
        public DateTime? Dob { get; set; }
        public short? Gender { get; set; }
        public short? IsAbrsmG5 { get; set; }
        public string G5Certification { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ReferrerLearnerId { get; set; }
        public string Photo { get; set; }
        public string Note { get; set; }
        public string MiddleName { get; set; }
        public int AppFormId { get; set; }

        public ICollection<ParentAppForm> ParentAppForm { get; set; }
    }
}
