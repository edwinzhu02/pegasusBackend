using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Lesson
    {
        public Lesson()
        {
            LearnerTransaction = new HashSet<LearnerTransaction>();
            TeacherTransaction = new HashSet<TeacherTransaction>();
        }

        public int LessonId { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public short? TeacherId { get; set; }
        public short OrgId { get; set; }
        public DateTime? BeginDate { get; set; }
        public short? IsCanceled { get; set; }
        public string Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CourseInstanceId { get; set; }
        public int GroupCourseInstanceId { get; set; }
        public byte? IsTrial { get; set; }

        public One2oneCourseInstance CourseInstance { get; set; }
        public GroupCourseInstance GroupCourseInstance { get; set; }
        public Learner Learner { get; set; }
        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
        public ICollection<LearnerTransaction> LearnerTransaction { get; set; }
        public ICollection<TeacherTransaction> TeacherTransaction { get; set; }
    }
}
