﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonConflictCheckController : BasicController
    {

        public LessonConflictCheckController(ablemusicContext ablemusicContext, ILogger<LessonConflictCheckController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/LessonConflictCheck/5
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new Result<Object>();
            List<Lesson> lessons = new List<Lesson>();
            try
            {
                lessons = await (from l in _ablemusicContext.Lesson
                                 join t in _ablemusicContext.Teacher on l.TeacherId equals t.TeacherId
                                 join lr in _ablemusicContext.Learner on l.LearnerId equals lr.LearnerId
                                 join o in _ablemusicContext.Org on l.OrgId equals o.OrgId
                                 join r in _ablemusicContext.Room on l.RoomId equals r.RoomId
                                 where l.IsCanceled != 1
                                 select new Lesson
                                 {
                                     LessonId = l.LessonId,
                                     LearnerId = l.LearnerId,
                                     RoomId = l.RoomId,
                                     TeacherId = l.TeacherId,
                                     OrgId = l.OrgId,
                                     CreatedAt = l.CreatedAt,
                                     CourseInstanceId = l.CourseInstanceId,
                                     GroupCourseInstanceId = l.GroupCourseInstanceId,
                                     IsTrial = l.IsTrial,
                                     BeginTime = l.BeginTime,
                                     EndTime = l.EndTime,
                                     InvoiceId = l.InvoiceId,
                                     IsConfirm = l.IsConfirm,
                                     TrialCourseId = l.TrialCourseId,
                                     IsChanged = l.IsChanged,
                                     Teacher = new Teacher
                                     {
                                         TeacherId = t.TeacherId,
                                         FirstName = t.FirstName,
                                         LastName = t.LastName,
                                     },
                                     Learner = new Learner
                                     {
                                         LearnerId = lr.LearnerId,
                                         FirstName = lr.FirstName,
                                         LastName = lr.LastName,
                                     },
                                     Org = new pegasusContext.Org
                                     {
                                         OrgId = o.OrgId,
                                         OrgName = o.OrgName,
                                     },
                                     Room = new Room
                                     {
                                         RoomId = r.RoomId,
                                         RoomName = r.RoomName
                                     }
                                 }
                                 ).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            if(lessons.Count < 1)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Lesson not found";
                return BadRequest(result);
            }

            List<List<Lesson>> roomConflictLessons = new List<List<Lesson>>();
            List<List<Lesson>> teacherConflictLessons = new List<List<Lesson>>();

            foreach(var lesson in lessons)
            {
                bool isChecked = false;
                foreach(var roomConflictLesson in roomConflictLessons)
                {
                    foreach(var rcl in roomConflictLesson)
                    {
                        if (rcl.LessonId == lesson.LessonId) isChecked = true;
                    }
                }

                if (!isChecked)
                {
                    List<Lesson> conflicts = lessons.FindAll(l => l.OrgId == lesson.OrgId && l.LessonId != lesson.LessonId && l.RoomId == lesson.RoomId &&
                        ((l.BeginTime > lesson.BeginTime && l.BeginTime < lesson.EndTime) ||
                        (l.EndTime > lesson.BeginTime && l.EndTime < lesson.EndTime) ||
                        (l.BeginTime < lesson.BeginTime && l.EndTime > lesson.EndTime)));
                    if (conflicts.Count > 0)
                    {
                        foreach (var c in conflicts)
                        {
                            var conflictPair = new List<Lesson>();
                            conflictPair.Add(lesson);
                            conflictPair.Add(c);
                            roomConflictLessons.Add(conflictPair);
                        }
                    }
                }
            }

            foreach (var lesson in lessons)
            {
                bool isChecked = false;
                foreach (var teacherConflictLesson in teacherConflictLessons)
                {
                    foreach (var tcl in teacherConflictLesson)
                    {
                        if (tcl.LessonId == lesson.LessonId) isChecked = true;
                    }
                }

                if (!isChecked)
                {
                    DateTime expandBeginTime = lesson.BeginTime.Value.AddMinutes(-60);
                    DateTime expandEndTime = lesson.EndTime.Value.AddMinutes(60);

                    List<Lesson> conflicts = lessons.FindAll(l => l.LessonId != lesson.LessonId && l.TeacherId == lesson.TeacherId && 
                        ((l.OrgId == lesson.OrgId && ((l.BeginTime > lesson.BeginTime && l.BeginTime < lesson.EndTime) || (l.EndTime > lesson.BeginTime && l.EndTime < lesson.EndTime) || (l.BeginTime < lesson.BeginTime && l.EndTime > lesson.EndTime)) || 
                        (l.OrgId != lesson.OrgId && ((l.BeginTime > expandBeginTime && l.BeginTime < expandEndTime) || (l.EndTime > expandBeginTime && l.EndTime < expandEndTime) || (l.BeginTime < expandBeginTime && l.EndTime > expandEndTime))))));
                    if (conflicts.Count > 0)
                    {
                        foreach (var c in conflicts)
                        {
                            var conflictPair = new List<Lesson>();
                            conflictPair.Add(lesson);
                            conflictPair.Add(c);
                            teacherConflictLessons.Add(conflictPair);
                        }
                    }
                }
            }

            var roomConflictLessonsView = new List<List<object>>();
            foreach(var rcl in roomConflictLessons)
            {
                var temp = new List<object>();
                foreach(var r in rcl)
                {
                    temp.Add(new
                    {
                        LessonId = r.LessonId,
                        LearnerId = r.LearnerId,
                        RoomId = r.RoomId,
                        TeacherId = r.TeacherId,
                        OrgId = r.OrgId,
                        CreatedAt = r.CreatedAt,
                        CourseInstanceId = r.CourseInstanceId,
                        GroupCourseInstanceId = r.GroupCourseInstanceId,
                        IsTrial = r.IsTrial,
                        BeginTime = r.BeginTime,
                        EndTime = r.EndTime,
                        InvoiceId = r.InvoiceId,
                        IsConfirm = r.IsConfirm,
                        TrialCourseId = r.TrialCourseId,
                        IsChanged = r.IsChanged,
                        TeacherName = r.Teacher.FirstName + " " + r.Teacher.LastName,
                        LearnerName = r.Learner.FirstName + " " + r.Learner.LastName,
                        OrgName = r.Org.OrgName,
                        RoomName = r.Room.RoomName,
                    });
                }
                roomConflictLessonsView.Add(temp);
            }
            var teacherConflictLessonsView = new List<List<object>>();
            foreach(var tcl in teacherConflictLessons)
            {
                var temp = new List<object>();
                foreach(var t in tcl)
                {
                    temp.Add(new
                    {
                        LessonId = t.LessonId,
                        LearnerId = t.LearnerId,
                        RoomId = t.RoomId,
                        TeacherId = t.TeacherId,
                        OrgId = t.OrgId,
                        CreatedAt = t.CreatedAt,
                        CourseInstanceId = t.CourseInstanceId,
                        GroupCourseInstanceId = t.GroupCourseInstanceId,
                        IsTrial = t.IsTrial,
                        BeginTime = t.BeginTime,
                        EndTime = t.EndTime,
                        InvoiceId = t.InvoiceId,
                        IsConfirm = t.IsConfirm,
                        TrialCourseId = t.TrialCourseId,
                        IsChanged = t.IsChanged,
                        TeacherName = t.Teacher.FirstName + " " + t.Teacher.LastName,
                        LearnerName = t.Learner.FirstName + " " + t.Learner.LastName,
                        OrgName = t.Org.OrgName,
                        RoomName = t.Room.RoomName,
                    });
                }
                teacherConflictLessonsView.Add(temp);
            }

            result.Data = new
            {
                RoomConflict = roomConflictLessonsView,
                TeacherConflict = teacherConflictLessonsView,
            };

            return Ok(result);
        }
    }
}
