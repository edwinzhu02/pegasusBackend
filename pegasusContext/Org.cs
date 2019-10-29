using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Org
    {
        public Org()
        {
            Amendment = new HashSet<Amendment>();
            AvailableDays = new HashSet<AvailableDays>();
            CashBox = new HashSet<CashBox>();
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Learner = new HashSet<Learner>();
            Lesson = new HashSet<Lesson>();
            LoginLog = new HashSet<LoginLog>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
            Payment = new HashSet<Payment>();
            Room = new HashSet<Room>();
            StaffOrg = new HashSet<StaffOrg>();
            Stock = new HashSet<Stock>();
            StockApplication = new HashSet<StockApplication>();
            StockOrder = new HashSet<StockOrder>();
        }

        public short OrgId { get; set; }
        public string OrgName { get; set; }
        public short? IsHeadoffice { get; set; }
        public short? IsActivate { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal? LocaltionX { get; set; }
        public decimal? LocaltionY { get; set; }
        public string Abbr { get; set; }
        public string BankName { get; set; }
        public string BankAccountNo { get; set; }
        public string GstNum { get; set; }

        public virtual ICollection<Amendment> Amendment { get; set; }
        public virtual ICollection<AvailableDays> AvailableDays { get; set; }
        public virtual ICollection<CashBox> CashBox { get; set; }
        public virtual ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        public virtual ICollection<Learner> Learner { get; set; }
        public virtual ICollection<Lesson> Lesson { get; set; }
        public virtual ICollection<LoginLog> LoginLog { get; set; }
        public virtual ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public virtual ICollection<Payment> Payment { get; set; }
        public virtual ICollection<Room> Room { get; set; }
        public virtual ICollection<StaffOrg> StaffOrg { get; set; }
        public virtual ICollection<Stock> Stock { get; set; }
        public virtual ICollection<StockApplication> StockApplication { get; set; }
        public virtual ICollection<StockOrder> StockOrder { get; set; }
    }
}
