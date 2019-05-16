using System;
using System.Collections.Generic;

namespace Pegasus_backend.Models
{
    public class OnetoOneCourseInstanceModel
    {
        public int id { get; set; }
        public int? CourseId { get; set; }
        public short? TeacherId { get; set; }
        public short? OrgId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public CourseScheduleModel Schedule { get; set; }
    }
}