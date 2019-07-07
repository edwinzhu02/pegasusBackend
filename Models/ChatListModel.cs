using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Models
{
    public class ChatListModel
    {
        public IList StaffList { get; set; }
        public IList TeacherList { get; set; }
        public IList LearnerList { get; set; }
        public IList OneToOneCourseList { get; set; }
        public IList OneToManyCourseList { get; set; }
    }
}
