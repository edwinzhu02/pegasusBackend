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
            InverseNewLesson = new HashSet<Lesson>();
            LearnerTransaction = new HashSet<LearnerTransaction>();
            Rating = new HashSet<Rating>();
            RemindLog = new HashSet<RemindLog>();
            SplittedLesson = new HashSet<SplittedLesson>();
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
        public string InvoiceNum { get; set; }
        public byte? IsConfirm { get; set; }
        public int? TrialCourseId { get; set; }
        public short? IsChanged { get; set; }
        public short? IsPaid { get; set; }
        public int? NewLessonId { get; set; }

        public virtual One2oneCourseInstance CourseInstance { get; set; }
        public virtual GroupCourseInstance GroupCourseInstance { get; set; }
        public virtual Learner Learner { get; set; }
        public virtual Lesson NewLesson { get; set; }
        public virtual Org Org { get; set; }
        public virtual Room Room { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual Course TrialCourse { get; set; }
        public virtual ICollection<AwaitMakeUpLesson> AwaitMakeUpLessonMissedLesson { get; set; }
        public virtual ICollection<AwaitMakeUpLesson> AwaitMakeUpLessonNewLesson { get; set; }
        public virtual ICollection<Lesson> InverseNewLesson { get; set; }
        public virtual ICollection<LearnerTransaction> LearnerTransaction { get; set; }
        public virtual ICollection<Rating> Rating { get; set; }
        public virtual ICollection<RemindLog> RemindLog { get; set; }
        public virtual ICollection<SplittedLesson> SplittedLesson { get; set; }
        public virtual ICollection<TeacherTransaction> TeacherTransaction { get; set; }
        public virtual ICollection<TodoList> TodoList { get; set; }
    }
}
