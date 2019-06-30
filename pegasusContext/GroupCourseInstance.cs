using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class GroupCourseInstance
    {
        public GroupCourseInstance()
        {
            CourseSchedule = new HashSet<CourseSchedule>();
            Invoice = new HashSet<Invoice>();
            InvoiceWaitingConfirm = new HashSet<InvoiceWaitingConfirm>();
            LearnerGroupCourse = new HashSet<LearnerGroupCourse>();
            Lesson = new HashSet<Lesson>();
            LessonRemain = new HashSet<LessonRemain>();
        }

        public int CourseId { get; set; }
        public short? TeacherId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? RoomId { get; set; }
        public short? OrgId { get; set; }
        public int GroupCourseInstanceId { get; set; }
        public short? IsActivate { get; set; }
        public short? IsStarted { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public Course Course { get; set; }
        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
        public ICollection<CourseSchedule> CourseSchedule { get; set; }
        public ICollection<Invoice> Invoice { get; set; }
        public ICollection<InvoiceWaitingConfirm> InvoiceWaitingConfirm { get; set; }
        public ICollection<LearnerGroupCourse> LearnerGroupCourse { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<LessonRemain> LessonRemain { get; set; }
    }
}
