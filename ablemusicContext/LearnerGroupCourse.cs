﻿using System;
using System.Collections.Generic;

namespace Pegasus_backend.ablemusicContext
{
    public partial class LearnerGroupCourse
    {
        public int? LearnerId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public int LearnerGroupCourseId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? IsActivate { get; set; }

        public GroupCourseInstance GroupCourseInstance { get; set; }
        public Learner Learner { get; set; }
    }
}
