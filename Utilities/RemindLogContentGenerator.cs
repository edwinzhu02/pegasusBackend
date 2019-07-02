using Pegasus_backend.pegasusContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Utilities
{
    public static class RemindLogContentGenerator
    {
        public static string CancelSingleLessonForTeacher(string courseName, Lesson lesson, string reason)
        {
            return "Your " + courseName + " lesson from " + lesson.BeginTime.ToString() +
                    " to " + lesson.EndTime.ToString() + " has been cancelled due to " + reason +
                    "\n Please click the following link to confirm. \n";
        }

        public static string CancelSingleLessonForLearner(string courseName, Lesson lesson, string reason)
        {
            return "Your " + courseName + " lesson from " + lesson.BeginTime.ToString() +
                  " to " + lesson.EndTime.ToString() + " has been cancelled due to " + reason +
                  "\n Please click the following link to confirm. \n";
        }
    }
}
