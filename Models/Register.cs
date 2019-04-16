using System;

namespace Pegasus_backend.Models
{
    public class Register
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public short Gender { get; set; }
        public DateTime dob { get; set; }
        public DateTime DateOfEnrollment { get; set; }
        public string ContactPhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ABRSM { get; set; }
        public string GuardianFirstName { get; set; }
        public string GuardianLastName { get; set; }
        public string GuardianRelationship { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianEmail { get; set; }
    }
}