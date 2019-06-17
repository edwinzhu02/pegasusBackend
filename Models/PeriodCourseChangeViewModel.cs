using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class PeriodCourseChangeViewModel
    {
        [Required(ErrorMessage = "UserId is required")]
        public short UserId { get; set; }
        [Required(ErrorMessage = "TeacherId is required")]
        public short? TeacherId { get; set; }
        [Required(ErrorMessage = "LearnerId is required")]
        public int LearnerId { get; set; }
        [Required(ErrorMessage = "BeginDate is required")]
        public DateTime BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        [Required(ErrorMessage = "InstanceId is required")]
        public int InstanceId { get; set; }
        [Required(ErrorMessage = "OrgId is required")]
        public int OrgId { get; set; }
        [Required(ErrorMessage = "DayOfWeek is required")]
        public short DayOfWeek { get; set; }
        [Required(ErrorMessage = "BeginTime is required")]
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
        [Required(ErrorMessage = "RoomId is required")]
        public short RoomId { get; set; }
        [Required(ErrorMessage = "IsTemporary is required")]
        public short IsTemporary { get; set; }
        public int? CourseScheduleId { get; set; }
    }
}
