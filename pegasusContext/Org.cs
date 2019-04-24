using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class Org
    {
        public Org()
        {
            Amendment = new HashSet<Amendment>();
            AvailableDays = new HashSet<AvailableDays>();
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Lesson = new HashSet<Lesson>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
            Room = new HashSet<Room>();
            Stock = new HashSet<Stock>();
            StockOrder = new HashSet<StockOrder>();
        }

        public short OrgId { get; set; }
        public string OrgName { get; set; }
        public short? IsHeadoffice { get; set; }
        public short? IsActivate { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [JsonIgnore]
        public ICollection<Amendment> Amendment { get; set; }
        [JsonIgnore]
        public ICollection<AvailableDays> AvailableDays { get; set; }
        [JsonIgnore]
        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        [JsonIgnore]
        public ICollection<Lesson> Lesson { get; set; }
        [JsonIgnore]
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        [JsonIgnore]
        public ICollection<Room> Room { get; set; }
        [JsonIgnore]
        public ICollection<Stock> Stock { get; set; }
        [JsonIgnore]
        public ICollection<StockOrder> StockOrder { get; set; }
    }
}
