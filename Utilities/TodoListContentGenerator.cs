﻿using Pegasus_backend.pegasusContext;
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

        public static string RearrangedSingleLessonWithOldLessonForLearner(Lesson oldLesson, Lesson newLesson, Learner learner,
            Teacher oldTeacher, Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom)
        {
            string content;
            content = "Inform learner " + learner.FirstName + " " + learner.LastName +
                " session given by teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                " " + oldRoom.RoomName + " has been rearanged to be given by teacher " + newTeacher.FirstName + " " +
                newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " + newLesson.EndTime.ToString() +
                " at " + newOrg.OrgName + " " + newRoom.RoomName;
            return content;
        }

        public static string RearrangedSingleLessonWithOldLessonForNewTeacher(Lesson oldLesson, Lesson newLesson,
            Teacher oldTeacher, Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom)
        {
            string content;
            content = "Inform teacher " + newTeacher.FirstName + " " + newTeacher.LastName +
                " session given by teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                " " + oldRoom.RoomName + " has been rearanged to be given by " + newTeacher.FirstName + " " +
                newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            return content;
        }

        public static string RearrangedSingleLessonWithOldLessonForOldTeacher(Lesson oldLesson, Lesson newLesson,
            Teacher oldTeacher, Teacher newTeacher, pegasusContext.Org oldOrg, pegasusContext.Org newOrg, Room oldRoom, Room newRoom)
        {
            string content;
            content = "Inform teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName +
                " session given by teacher " + oldTeacher.FirstName + " " + oldTeacher.LastName + " from " +
                oldLesson.BeginTime.ToString() + " to " + oldLesson.EndTime.ToString() + " at " + oldOrg.OrgName +
                " " + oldRoom.RoomName + " has been rearanged to be given by " + newTeacher.FirstName + " " +
                newTeacher.LastName + " from " + newLesson.BeginTime.ToString() + " to " +
                newLesson.EndTime.ToString() + " at " + newOrg.OrgName + " " + newRoom.RoomName;
            return content;
        }
    }
}
