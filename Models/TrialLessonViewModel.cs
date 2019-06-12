using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Pegasus_backend.Models
{
    public class TrialLessonViewModel
    {
        [Required(ErrorMessage = "LearnerId is required")]
        public int? LearnerId { get; set; }
        [Required(ErrorMessage = "RoomId is required")]
        public short? RoomId { get; set; }
        [Required(ErrorMessage = "TeacherId is required")]
        public short? TeacherId { get; set; }
        [Required(ErrorMessage = "OrgId is required")]
        public short OrgId { get; set; }
        [Required(ErrorMessage = "BeginTime is required")]
        public DateTime? BeginTime { get; set; }
        [Required(ErrorMessage = "EndTime is required")]
        public DateTime? EndTime { get; set; }
        [Required(ErrorMessage = "PaymentMethod is required")]
        public byte? PaymentMethod { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        public decimal? Amount { get; set; }
        [Required(ErrorMessage = "StaffId is required")]
        public short? StaffId { get; set; }
        [Required(ErrorMessage = "TrialCourseId is required")]
        public int? TrialCourseId { get; set; }

    }
}
