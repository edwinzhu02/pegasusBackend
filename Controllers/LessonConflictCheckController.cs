using System;
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

        public LessonConflictCheckController(ablemusicContext ablemusicContext, ILogger<ValuesController> log) : base(ablemusicContext, log)
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
                lessons = await _ablemusicContext.Lesson.Where(l => l.IsCanceled != 1).ToListAsync();
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
                bool isChecded = false;
                foreach(var roomConflictLesson in roomConflictLessons)
                {
                    foreach(var rcl in roomConflictLesson)
                    {
                        if (rcl.LessonId == lesson.LessonId) isChecded = true;
                    }
                }

                if (!isChecded)
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
                bool isChecded = false;
                foreach (var teacherConflictLesson in teacherConflictLessons)
                {
                    foreach (var tcl in teacherConflictLesson)
                    {
                        if (tcl.LessonId == lesson.LessonId) isChecded = true;
                    }
                }

                if (!isChecded)
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

            result.Data = new
            {
                RoomConflict = roomConflictLessons,
                TeacherConflict = teacherConflictLessons,
            };

            return Ok(result);
        }
    }
}
