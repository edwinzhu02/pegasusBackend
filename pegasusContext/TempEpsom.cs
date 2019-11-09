using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TempEpsom
    {
        public int Id { get; set; }
        public string No { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Dob { get; set; }
        public string Address { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        public string Email { get; set; }
        public string ParentName { get; set; }
        public string ClassDate { get; set; }
        public string Tutor { get; set; }
        public string StartDate { get; set; }
    }
}
