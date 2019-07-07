using Pegasus_backend.Models;
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

        public static string RearrangedSingleLessonWithOldLessonForLearner(Lesson oldLesson, Lesson newLesson, Teacher oldTeacher, 
            Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom, string courseName)
        {
            string content;
            content = "Your " + courseName + " lesson given by " + oldTeacher.FirstName + " " +
                oldTeacher.LastName + " from " + oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() +
                " at " + oldOrg.OrgName + " " + oldRoom.RoomName + " has been rearranged to be given by teacher " +
                newTeacher.FirstName + " " + newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            return content;
        }

        public static string RearrangedSingleLessonWithOldLessonForOldTeacher(Lesson oldLesson, Lesson newLesson, Teacher oldTeacher,
            Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom, string courseName)
        {
            string content;
            content = "The " + courseName + " lesson given by " + oldTeacher.FirstName + " " +
                oldTeacher.LastName + " from " + oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() +
                " at " + oldOrg.OrgName + " " + oldRoom.RoomName + " has been rearranged to be given by teacher " +
                newTeacher.FirstName + " " + newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            return content;
        }

        public static string RearrangedSingleLessonWithOldLessonForNewTeacher(Lesson oldLesson, Lesson newLesson, Teacher oldTeacher,
            Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom, string courseName)
        {
            string content;
            content = "The " + courseName + " lesson given by " + oldTeacher.FirstName + " " +
                oldTeacher.LastName + " from " + oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() +
                " at " + oldOrg.OrgName + " " + oldRoom.RoomName + " has been rearranged to be given by teacher " +
                newTeacher.FirstName + " " + newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            return content;
        }

        public static string LessonRescheduleForLearner(Lesson lesson, Learner learner, List<Lesson> appendLessons, string courseName)
        {
            string content = "Inform learner " + learner.FirstName + " " + learner.LastName +
                " the " + courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been rescheduled. Lesson remain hours are append to the following lessions: \n";
            foreach (var appendLesson in appendLessons)
            {
                content += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            return content;
        }

        public static string LessonRescheduleForTeacher(Lesson lesson, List<Lesson> appendLessons, Teacher teacher, string courseName)
        {
            string content = "Inform teacher " + teacher.FirstName + " " + teacher.LastName +
                " the " + courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                " has been rescheduled. Lesson Remain hours are append to the following lessons: \n";
            foreach (var appendLesson in appendLessons)
            {
                content += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            return content;
        }

        public static string PeriodCourseChangeForLearner(dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj)
        {
            string content = "Inform learner " + courseInfo.LearnerFirstName + " " + courseInfo.LearnerLastName +
                " the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                TimeConvertor.getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + TimeConvertor.getDayOfWeek(inputObj.DayOfWeek) + " ";
            return content;
        }

        public static string PeriodCourseChangeForTeacher(dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj)
        {
            string content = "Inform teacher " + courseInfo.TeacherFirstName + " " + courseInfo.TeacherLastName +
                " the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                TimeConvertor.getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + TimeConvertor.getDayOfWeek(inputObj.DayOfWeek) + " ";
            return content;
        }
    }
}
