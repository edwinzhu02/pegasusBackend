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
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Learner = new HashSet<Learner>();
            Lesson = new HashSet<Lesson>();
            LoginLog = new HashSet<LoginLog>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
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

        public ICollection<Amendment> Amendment { get; set; }
        public ICollection<AvailableDays> AvailableDays { get; set; }
        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        public ICollection<Learner> Learner { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<LoginLog> LoginLog { get; set; }
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public ICollection<Room> Room { get; set; }
        public ICollection<StaffOrg> StaffOrg { get; set; }
        public ICollection<Stock> Stock { get; set; }
        public ICollection<StockApplication> StockApplication { get; set; }
        public ICollection<StockOrder> StockOrder { get; set; }
    }
}
