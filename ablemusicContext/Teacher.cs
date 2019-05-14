using System;
using System.Collections.Generic;

namespace Pegasus_backend.ablemusicContext
{
    public partial class Teacher
    {
        public Teacher()
        {
            AvailableDays = new HashSet<AvailableDays>();
            GroupCourseInstance = new HashSet<GroupCourseInstance>();
            Lesson = new HashSet<Lesson>();
            One2oneCourseInstance = new HashSet<One2oneCourseInstance>();
            RemindLog = new HashSet<RemindLog>();
            TeacherCourse = new HashSet<TeacherCourse>();
            TeacherLanguage = new HashSet<TeacherLanguage>();
            TeacherQualificatiion = new HashSet<TeacherQualificatiion>();
            TodoList = new HashSet<TodoList>();
        }

        public short TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Dob { get; set; }
        public short? Gender { get; set; }
        public string IrdNumber { get; set; }
        public short? IdType { get; set; }
        public string IdPhoto { get; set; }
        public string IdNumber { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public string Visa { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public short? IsActivate { get; set; }
        public short? UserId { get; set; }
        public byte? Level { get; set; }

        public User User { get; set; }
        public ICollection<AvailableDays> AvailableDays { get; set; }
        public ICollection<GroupCourseInstance> GroupCourseInstance { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public ICollection<RemindLog> RemindLog { get; set; }
        public ICollection<TeacherCourse> TeacherCourse { get; set; }
        public ICollection<TeacherLanguage> TeacherLanguage { get; set; }
        public ICollection<TeacherQualificatiion> TeacherQualificatiion { get; set; }
        public ICollection<TodoList> TodoList { get; set; }
    }
}
