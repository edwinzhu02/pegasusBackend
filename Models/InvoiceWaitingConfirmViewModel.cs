using System;
using System.ComponentModel.DataAnnotations;


namespace Pegasus_backend.Models
{
    public class InvoiceWaitingConfirmViewModel
    {
        [Required(ErrorMessage = "InvoiceNum id is required")]
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
        public short? IsActivate { get; set; }
        public short? IsEmailSent { get; set; }
        public short? IsConfirmed { get; set; }
    }
}
