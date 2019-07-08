using System;
using System.ComponentModel.DataAnnotations;


namespace Pegasus_backend.Models
{
    public class LessonViewModel
    {
        public int? LessonId { get; set; }
        //[Required(ErrorMessage = "LearnerId is required")]
        public int? LearnerId { get; set; }
        [Required(ErrorMessage = "RoomId is required")]
        public short? RoomId { get; set; }
        [Required(ErrorMessage = "TeacherId is required")]
        public short? TeacherId { get; set; }
        [Required(ErrorMessage = "OrgId is required")]
        public short OrgId { get; set; }
        public short? IsCanceled { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public byte? IsTrial { get; set; }
        [Required(ErrorMessage = "BeginTime is required")]
        public DateTime? BeginTime { get; set; }
        public int? InvoiceId { get; set; }
    }
}
