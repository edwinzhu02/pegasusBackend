using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class GroupCourseInstance
    {
        public GroupCourseInstance()
        {
            Invoice = new HashSet<Invoice>();
            LearnerGroupCourse = new HashSet<LearnerGroupCourse>();
            Lesson = new HashSet<Lesson>();
            LessonRemain = new HashSet<LessonRemain>();
        }

        public int CourseId { get; set; }
        public short? TeacherId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte? DayOfWeek { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public short? RoomId { get; set; }
        public short? OrgId { get; set; }
        public int GroupCourseInstanceId { get; set; }

        public Course Course { get; set; }
        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
        [JsonIgnore]
        public ICollection<Invoice> Invoice { get; set; }
        [JsonIgnore]
        public ICollection<LearnerGroupCourse> LearnerGroupCourse { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<LessonRemain> LessonRemain { get; set; }
    }
}
