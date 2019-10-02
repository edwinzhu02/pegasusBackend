using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class One2oneCourseInstance
    {
        public One2oneCourseInstance()
        {
            Amendment = new HashSet<Amendment>();
            AwaitMakeUpLesson = new HashSet<AwaitMakeUpLesson>();
            CourseSchedule = new HashSet<CourseSchedule>();
            Invoice = new HashSet<Invoice>();
            InvoiceWaitingConfirm = new HashSet<InvoiceWaitingConfirm>();
            Lesson = new HashSet<Lesson>();
            LessonRemain = new HashSet<LessonRemain>();
        }

        public int CourseInstanceId { get; set; }
        public int? CourseId { get; set; }
        public short? TeacherId { get; set; }
        public short? OrgId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public virtual Course Course { get; set; }
        public virtual Learner Learner { get; set; }
        public virtual Org Org { get; set; }
        public virtual Room Room { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual ICollection<Amendment> Amendment { get; set; }
        public virtual ICollection<AwaitMakeUpLesson> AwaitMakeUpLesson { get; set; }
        public virtual ICollection<CourseSchedule> CourseSchedule { get; set; }
        public virtual ICollection<Invoice> Invoice { get; set; }
        public virtual ICollection<InvoiceWaitingConfirm> InvoiceWaitingConfirm { get; set; }
        public virtual ICollection<Lesson> Lesson { get; set; }
        public virtual ICollection<LessonRemain> LessonRemain { get; set; }
    }
}
