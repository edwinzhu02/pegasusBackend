using Org.BouncyCastle.Asn1.Cms;

namespace Pegasus_backend.Models
{
    public class GroupCourse
    {
        public string Course { get; set; }
        public string TeacherName { get; set; }
        public Time CourseTime { get; set; }
        public string Location { get; set; }
    }
}