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

        public static string RearrangedSingleLessonWithoutOldLessonForLearner(Learner learner, Lesson lesson, pegasusContext.Org org, Room room, Teacher teacher)
        {
            string content;
            content = "Inform learner " + learner.FirstName + " " + learner.LastName + "new lesson has been arranged at " + org.OrgName + " " +
                room.RoomName + " from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() + " given by teacher " +
                teacher.FirstName + " " + teacher.LastName;
            return content;
        }

        public static string RearrangedSingleLessonWithoutOldLessonForTeacher(Learner learner, Lesson lesson, pegasusContext.Org org, Room room, Teacher teacher)
        {
            string content;
            content = "Inform teacher " + teacher.FirstName + " " + teacher.LastName + "new lesson has been arranged at " + org.OrgName + " " +
                room.RoomName + " from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() + " for learner " +
                teacher.FirstName + " " + teacher.LastName;
            return content;
        }
    }
}
