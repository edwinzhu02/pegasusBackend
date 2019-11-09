using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Invoice
    {
        public Invoice()
        {
            Payment = new HashSet<Payment>();
        }

        public int InvoiceId { get; set; }
        public string InvoiceNum { get; set; }
        public decimal? LessonFee { get; set; }
        public decimal? ConcertFee { get; set; }
        public decimal? NoteFee { get; set; }
        public decimal? Other1Fee { get; set; }
        public decimal? Other2Fee { get; set; }
        public decimal? Other3Fee { get; set; }
        public int? LearnerId { get; set; }
        public string LearnerName { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TotalFee { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? PaidFee { get; set; }
        public decimal? OwingFee { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? IsPaid { get; set; }
        public short? TermId { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public int? LessonQuantity { get; set; }
        public string CourseName { get; set; }
        public string ConcertFeeName { get; set; }
        public string LessonNoteFeeName { get; set; }
        public string Other1FeeName { get; set; }
        public string Other2FeeName { get; set; }
        public string Other3FeeName { get; set; }
        public short? IsActive { get; set; }
        public string Comment { get; set; }
        public decimal? Credit { get; set; }
        public string Other4FeeName { get; set; }
        public decimal? Other4Fee { get; set; }
        public string Other5FeeName { get; set; }
        public decimal? Other5Fee { get; set; }
        public string Other6FeeName { get; set; }
        public decimal? Other6Fee { get; set; }
        public string Other7FeeName { get; set; }
        public decimal? Other7Fee { get; set; }
        public string Other8FeeName { get; set; }
        public decimal? Other8Fee { get; set; }
        public string Other9FeeName { get; set; }
        public decimal? Other9Fee { get; set; }
        public string Other10FeeName { get; set; }
        public decimal? Other10Fee { get; set; }
        public string Other11FeeName { get; set; }
        public decimal? Other11Fee { get; set; }
        public string Other12FeeName { get; set; }
        public decimal? Other12Fee { get; set; }
        public string Other13FeeName { get; set; }
        public decimal? Other13Fee { get; set; }
        public string Other14FeeName { get; set; }
        public decimal? Other14Fee { get; set; }
        public string Other15FeeName { get; set; }
        public decimal? Other15Fee { get; set; }
        public string Other16FeeName { get; set; }
        public decimal? Other16Fee { get; set; }
        public string Other17FeeName { get; set; }
        public decimal? Other17Fee { get; set; }
        public string Other18FeeName { get; set; }
        public decimal? Other18Fee { get; set; }

        public virtual One2oneCourseInstance CourseInstance { get; set; }
        public virtual GroupCourseInstance GroupCourseInstance { get; set; }
        public virtual Learner Learner { get; set; }
        public virtual Term Term { get; set; }
        public virtual ICollection<Payment> Payment { get; set; }
    }
}
