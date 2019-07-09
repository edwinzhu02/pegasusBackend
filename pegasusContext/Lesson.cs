using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Lesson
    {
        public Lesson()
        {
            AwaitMakeUpLessonMissedLesson = new HashSet<AwaitMakeUpLesson>();
            AwaitMakeUpLessonNewLesson = new HashSet<AwaitMakeUpLesson>();
            LearnerTransaction = new HashSet<LearnerTransaction>();
            RemindLog = new HashSet<RemindLog>();
            TeacherTransaction = new HashSet<TeacherTransaction>();
            TodoList = new HashSet<TodoList>();
        }

        public int LessonId { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public short? TeacherId { get; set; }
        public short OrgId { get; set; }
        public short? IsCanceled { get; set; }
        public string Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public byte? IsTrial { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? InvoiceId { get; set; }
        public byte? IsConfirm { get; set; }
        public int? TrialCourseId { get; set; }
        public short? IsChanged { get; set; }

        public One2oneCourseInstance CourseInstance { get; set; }
        public GroupCourseInstance GroupCourseInstance { get; set; }
        public Invoice Invoice { get; set; }
        public Learner Learner { get; set; }
        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
        public Course TrialCourse { get; set; }
        public ICollection<AwaitMakeUpLesson> AwaitMakeUpLessonMissedLesson { get; set; }
        public ICollection<AwaitMakeUpLesson> AwaitMakeUpLessonNewLesson { get; set; }
        public ICollection<LearnerTransaction> LearnerTransaction { get; set; }
        public ICollection<RemindLog> RemindLog { get; set; }
        public ICollection<TeacherTransaction> TeacherTransaction { get; set; }
        public ICollection<TodoList> TodoList { get; set; }
    }
}
