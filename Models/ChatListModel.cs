using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Models
{
    public class ChatListModel
    {
        public List<Staff> StaffList { get; set; }
        public List<Teacher> TeacherList { get; set; }
        public List<Learner> LearnerList { get; set; }
        public List<Lesson> OneToOneCourseList { get; set; }
        public List<Lesson> OneToManyCourseList { get; set; }
    }
}
