using System;

namespace Pegasus_backend.Models
{
    public class CourseScheduleModel
    {
        public byte? DayOfWeek { get; set; }
        public TimeSpan BeginTime { get; set; }
    }
}