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
    }
}
