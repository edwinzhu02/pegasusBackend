﻿using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class One2oneCourseInstance
    {
        public One2oneCourseInstance()
        {
            Amendment = new HashSet<Amendment>();
            Invoice = new HashSet<Invoice>();
            Lesson = new HashSet<Lesson>();
            LessonRemain = new HashSet<LessonRemain>();
        }

        public int CourseInstanceId { get; set; }
        public int? CourseId { get; set; }
        public short? TeacherId { get; set; }
        public byte? DayOfWeek { get; set; }
        public short? OrgId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public Course Course { get; set; }
        public Learner Learner { get; set; }
        public Org Org { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
        public ICollection<Amendment> Amendment { get; set; }
        public ICollection<Invoice> Invoice { get; set; }
        public ICollection<Lesson> Lesson { get; set; }
        public ICollection<LessonRemain> LessonRemain { get; set; }
    }
}
