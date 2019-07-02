using Pegasus_backend.pegasusContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Utilities
{
    public static class TodoListContentGenerator
    {
        public static string CancelSingleLessonForTeacher(Teacher teacher, Lesson lesson, string reason)
        {
            return "Inform teacher " + teacher.FirstName + " " + teacher.LastName +
                " session from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been cancelled due to " + reason;
        }

        public static string CancelSingleLessonForLearner(Learner learner, Lesson lesson, string reason)
        {
            return "Inform learner " + learner.FirstName + " " + learner.LastName +
                " session from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been cancelled due to " + reason; 
        }
    }
}
