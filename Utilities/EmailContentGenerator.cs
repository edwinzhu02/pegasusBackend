using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Utilities
{
    public static class EmailContentGenerator
    {
        public static string CancelLesson(string name, string courseName, Lesson lesson, string reason, string confirmURL)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>Your " +
                    courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                    " has been cancelled due to " + reason + ". Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }

        public static string RearrangeLesson(string name, string courseName, Lesson lesson, string confirmURL, pegasusContext.Org org, Room room)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>You have a new rearranged lesson " +
                    courseName + " from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                    " at " + org.OrgName + " " + room.RoomName + ".</p><p>Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }

        public static string Dayoff(string name, dynamic courseSchedule, LearnerDayoffViewModel inputObj, string confirmURL)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>This is to inform you that the learner " + courseSchedule.LearnerFirstName +
                " " + courseSchedule.LearnerLastName + " has been taken the dayoff from " + inputObj.BeginDate.ToString() + " to " +
                inputObj.EndDate.ToString() + ". The course " + courseSchedule.CourseName + " in the period is canceled. </p>";

            mailContent += "<p> Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }

        public static string RearrangeLessonWithOld(string name, string courseName, string confirmURL, Lesson oldLesson, Teacher oldTeacher, 
            Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>The " +
                    courseName + " lesson given by " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                    oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                    " " + oldRoom.RoomName +
                    " has been rearranged to be given by" + newTeacher.FirstName + " " + newTeacher.LastName + " at " +
                    newOrg.OrgName + " " + newRoom.RoomName + ". Please click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";

            return mailContent;
        }

        public static string LessonReschedule(string name, string courseName, Lesson lesson, string reason, string confirmURL, List<Lesson> appendLessons)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>Your " +
                    courseName + " lesson from " + lesson.BeginTime.ToString() + " to " + lesson.EndTime.ToString() +
                    " has been rescheduled due to " + reason + ". Lesson Remain hours are append to the following lessons: \n";
            foreach (var appendLesson in appendLessons)
            {
                mailContent += "Lesson from " + appendLesson.BeginTime + " to " + appendLesson.EndTime + "\n";
            }
            mailContent += "\nPlease click the following button to confirm. </p>" +
                    "<a style='background-color:#4CAF50; color:#FFFFFF' href='" + confirmURL +
                    "' target='_blank'>Confirm</a></div>";
            return mailContent;
        }

        public static string PeriodCourseChange(string name, dynamic courseInfo, dynamic newCourseInfo, PeriodCourseChangeViewModel inputObj)
        {
            string mailContent = "<div><p>Dear " + name + "</p>" + "<p>This is to inform you that the course " + courseInfo.CourseName + " at " + courseInfo.OrgName + " " + courseInfo.RoomName + " from " +
                TimeConvertor.getDayOfWeek(courseInfo.DayOfWeek) + " " + courseInfo.BeginTime + " to " + courseInfo.EndTime + " has been changed to " +
                newCourseInfo.newOrg.OrgName + " " + newCourseInfo.newRoom.RoomName + " from " + inputObj.BeginTime + " to " + inputObj.EndTime +
                "on " + TimeConvertor.getDayOfWeek(inputObj.DayOfWeek) + " ";
            mailContent += inputObj.IsTemporary == 1 ? "for the period between " + inputObj.BeginDate + " to " +
                inputObj.EndDate + " Temporarily" : "from " + inputObj.BeginDate + "permanently</p>";
            return mailContent;
        }
    }
}
