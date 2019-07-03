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

        public static string DayOffForLearner(dynamic courseSchedule, string endDate)
        {
            string content;
            content = "Inform learner " + courseSchedule.LearnerFirstName + " " + courseSchedule.LearnerLastName +
                " the period of dayoff for the course: " + courseSchedule.CourseName + " will finish soon by " + endDate;
            return content;
        }

        public static string DayOffForTeacher(dynamic courseSchedule, string endDate)
        {
            string content;
            content = "Inform teacher " + courseSchedule.TeacherFirstName + " " + courseSchedule.TeacherLastName +
                " the learner " + courseSchedule.LearnerFirstName + " " + courseSchedule.LearnerLastName + "'s dayoff for the course " +
                courseSchedule.CourseName + " will finish soon by " + endDate;
            return content;
        }
    }
}
