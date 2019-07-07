using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Learner
    {
        public Learner()
        {
            Amendment = new HashSet<Amendment>();
            Invoice = new HashSet<Invoice>();
            InvoiceWaitingConfirm = new HashSet<InvoiceWaitingConfirm>();
            LearnerGroupCourse = new HashSet<LearnerGroupCourse>();
            LearnerOthers = new HashSet<LearnerOthers>();
            Lesson = new HashSet<Lesson>();
            LessonRemain = new HashSet<LessonRemain>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
            Parent = new HashSet<Parent>();
            Payment = new HashSet<Payment>();
            Rating = new HashSet<Rating>();
            RemindLog = new HashSet<RemindLog>();
            SoldTransaction = new HashSet<SoldTransaction>();
            TodoList = new HashSet<TodoList>();
        }

        public int LearnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? EnrollDate { get; set; }
        public string ContactNum { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public short? IsUnder18 { get; set; }
        public DateTime? Dob { get; set; }
        public short? Gender { get; set; }
        public short? IsAbrsmG5 { get; set; }
        public string G5Certification { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ReferrerLearnerId { get; set; }
        public string Photo { get; set; }
        public string Note { get; set; }
        public string MiddleName { get; set; }
        public byte? LevelType { get; set; }
        public short? UserId { get; set; }
        public short? OrgId { get; set; }
        public short? IsActive { get; set; }
        public byte? LearnerLevel { get; set; }
        public byte? PaymentPeriod { get; set; }
        public string Referrer { get; set; }
        public string FormUrl { get; set; }
        public string OtherfileUrl { get; set; }
        public string Comment { get; set; }

        public Org Org { get; set; }
        public User User { get; set; }
        public ICollection<Amendment> Amendment { get; set; }
        public ICollection<Invoice> Invoice { get; set; }
        public ICollection<InvoiceWaitingConfirm> InvoiceWaitingConfirm { get; set; }
        public ICollection<LearnerGroupCourse> LearnerGroupCourse { get; set; }
        public ICollection<LearnerOthers> LearnerOthers { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<LessonRemain> LessonRemain { get; set; }
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public ICollection<Parent> Parent { get; set; }
        public ICollection<Payment> Payment { get; set; }
        public ICollection<Rating> Rating { get; set; }
        public ICollection<RemindLog> RemindLog { get; set; }
        public ICollection<SoldTransaction> SoldTransaction { get; set; }
        public ICollection<TodoList> TodoList { get; set; }
    }
}
