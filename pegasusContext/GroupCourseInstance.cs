using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class GroupCourseInstance
    {
        public GroupCourseInstance()
        {
            AwaitMakeUpLesson = new HashSet<AwaitMakeUpLesson>();
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
        public byte? PromotionId { get; set; }

        public virtual Course Course { get; set; }
        public virtual Org Org { get; set; }
        public virtual Promotion Promotion { get; set; }
        public virtual Room Room { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual ICollection<AwaitMakeUpLesson> AwaitMakeUpLesson { get; set; }
        public virtual ICollection<CourseSchedule> CourseSchedule { get; set; }
        public virtual ICollection<Invoice> Invoice { get; set; }
        public virtual ICollection<InvoiceWaitingConfirm> InvoiceWaitingConfirm { get; set; }
        public virtual ICollection<LearnerGroupCourse> LearnerGroupCourse { get; set; }
        public virtual ICollection<Lesson> Lesson { get; set; }
        public virtual ICollection<LessonRemain> LessonRemain { get; set; }
    }
}
