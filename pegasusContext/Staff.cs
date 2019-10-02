using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Staff
    {
        public Staff()
        {
            NoticesFromStaff = new HashSet<Notices>();
            NoticesToStaff = new HashSet<Notices>();
            Payment = new HashSet<Payment>();
            SplittedLesson = new HashSet<SplittedLesson>();
            StaffOrg = new HashSet<StaffOrg>();
            StockApplication = new HashSet<StockApplication>();
            StockOrder = new HashSet<StockOrder>();
        }

        public string FirstName { get; set; }
        public string Visa { get; set; }
        public string LastName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? Dob { get; set; }
        public short? IsActivate { get; set; }
        public short? Gender { get; set; }
        public short? UserId { get; set; }
        public string IrdNumber { get; set; }
        public short? IdType { get; set; }
        public string IdPhoto { get; set; }
        public string IdNumber { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public short StaffId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Notices> NoticesFromStaff { get; set; }
        public virtual ICollection<Notices> NoticesToStaff { get; set; }
        public virtual ICollection<Payment> Payment { get; set; }
        public virtual ICollection<SplittedLesson> SplittedLesson { get; set; }
        public virtual ICollection<StaffOrg> StaffOrg { get; set; }
        public virtual ICollection<StockApplication> StockApplication { get; set; }
        public virtual ICollection<StockOrder> StockOrder { get; set; }
    }
}
