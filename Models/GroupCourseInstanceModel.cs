using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Models
{
    public class GroupCourseInstanceModel
    {
        [Required(ErrorMessage = "CourseId is required")]
        public int CourseId { get; set; }
        [Required(ErrorMessage = "TeacherId is required")]
        public short? TeacherId { get; set; }
        [Required(ErrorMessage = "BeginDate is required")]
        public DateTime? BeginDate { get; set; }
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime? EndDate { get; set; }
        [Required(ErrorMessage = "RoomId is required")]
        public short? RoomId { get; set; }
        [Required(ErrorMessage = "OrgId is required")]
        public short? OrgId { get; set; }
        [Required(ErrorMessage = "CourseSchedule is required")]
        public List<CourseSchedule> CourseSchedule { get; set; }
    }
}