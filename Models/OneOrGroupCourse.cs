using System;
using System.Collections.Generic;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Models
{
    public class OneOrGroupCourse
    {
        public int isOne2one { get; set; }
        public int CourseInstanceId { get; set; }
        public short? RoomId { get; set; }
        public short? TeacherId { get; set; }
        public short? OrgId { get; set; }
        public int GroupCourseInstanceId { get; set; }


    }
}