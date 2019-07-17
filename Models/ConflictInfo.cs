using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Models
{
    public class ConflictInfo
    {
        public int CourseScheduleId { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public byte? DayOfWeek { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? CourseId { get; set; }
        public short? TeacherId { get; set; }
        public short? OrgId { get; set; }
        public short? RoomId { get; set; }
        public string CourseName { get; set; }
        public string OrgName { get; set; }
        public string RoomName { get; set; }
        public string TeacherFirstName { get; set; }
        public string TeacherLastName { get; set; }
    }
}
