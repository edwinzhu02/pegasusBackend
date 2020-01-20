using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherAvailableCheckController : BasicController
    {
        public TeacherAvailableCheckController(ablemusicContext ablemusicContext, ILogger<TeacherAvailableCheckController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/TeacherAvailableCheck/5
        [HttpGet("Available/{orgId}/{StartDate}")]
        public async Task<IActionResult> GetOrgAvailableTeacher(short orgId, string StartDate)
        {
            Result<List<Object>> result = new Result<List<Object>>();
            DateTime beginDate = DateTime.Parse(StartDate);
            short dayOfWeek = DayofWeekToInt(beginDate.DayOfWeek);
            List<short?> teacherList = await _ablemusicContext.AvailableDays.Where(t => t.OrgId == orgId || t.DayOfWeek == dayOfWeek).
                    Select(t => t.TeacherId).ToListAsync();
            // var lesson = await _ablemusicContext.Lesson.
            //                     Where(l => teacherList.Contains(l.TeacherId));
            if (result.Data.Count>=1)
                return Ok(result);
            else{
                result.IsSuccess = false;
                return NotFound(result);
            }
        } 

        // GET: api/TeacherAvailableCheck/5
        [HttpGet("[action]/{orgId}/{StartDate}")]
        public async Task<IActionResult> GetOrgTeacher(short orgId, string StartDate)
        {
            Result<List<Object>> result = new Result<List<Object>>();
            result.Data = new List<object>();
            DateTime beginDate = DateTime.Parse(StartDate);
            short dayOfWeek = DayofWeekToInt(beginDate.DayOfWeek);

            var teacherList = await _ablemusicContext.AvailableDays
                .Include(s => s.Teacher)
                .Where(s => s.OrgId == orgId && s.Teacher.IsActivate == 1)
                .Select(s => new
                {
                    s.Teacher.TeacherId,
                    s.Teacher.FirstName,
                    s.Teacher.LastName
                })
                .Distinct()
                .ToListAsync();

            foreach (var teacher in teacherList){
                var res = await GetTeacherAvailableTime((int)teacher.TeacherId, StartDate);
                if (res.IsSuccess==true)
                    result.Data.Add( new {
                        teacherId = teacher.TeacherId,
                        FirstName = teacher.FirstName,
                        LastName = teacher.LastName,
                        data = res.Data
                        });
            }
            if (result.Data.Count>=1)
                return Ok(result);
            else{
                result.IsSuccess = false;
                return NotFound(result);
            }
        }

        [HttpGet("{teacherId}/{StartDate}")]
        public async Task<IActionResult> GetTeacher(int teacherId, string StartDate)
        {
            Result<Object> result = new Result<Object>();
            result =await GetTeacherAvailableTime(teacherId,StartDate);
            if (result.IsSuccess==true)
                return Ok(result);
            else    
                return NotFound(result);
        }
        private async Task<Result<Object>> GetTeacherAvailableTime(int teacherId, string StartDate)
        {
            Result<Object> result = new Result<Object>();
            Teacher teacher;
            DateTime beginDate;
            List<AvailableDays> availableDays;
            List<Amendment> amendments;
            dynamic arrangedOtOSchedule;
            dynamic arrangedGroupSchedule;
            List<dynamic> customisedAvailableDays;
            List<dynamic> customisedArranged;
            List<dynamic> customisedDayoff = new List<dynamic>();
            List<dynamic> customisedTempChange = new List<dynamic>();
            List<Lesson> arrangedLessons = new List<Lesson>();
            
            try
            {
                beginDate = DateTime.Parse(StartDate);
                teacher = await _ablemusicContext.Teacher.Where(t => t.TeacherId == teacherId).FirstOrDefaultAsync();
                availableDays = await (from ad in _ablemusicContext.AvailableDays
                                       join o in _ablemusicContext.Org on ad.OrgId equals o.OrgId
                                       where ad.TeacherId == teacherId
                                       select new AvailableDays
                                       {
                                           TeacherId = ad.TeacherId,
                                           AvailableDaysId = ad.AvailableDaysId,
                                           DayOfWeek = ad.DayOfWeek,
                                           CreatedAt = ad.CreatedAt,
                                           OrgId = ad.OrgId,
                                           Org = new pegasusContext.Org
                                           {
                                               OrgId = o.OrgId,
                                               OrgName = o.OrgName,
                                           }
                                       }).ToListAsync();
                arrangedOtOSchedule = await (from l in _ablemusicContext.Learner
                                             join oto in _ablemusicContext.One2oneCourseInstance on l.LearnerId equals oto.LearnerId
                                             join cs in _ablemusicContext.CourseSchedule on oto.CourseInstanceId equals cs.CourseInstanceId
                                             join o in _ablemusicContext.Org on oto.OrgId equals o.OrgId
                                             where oto.TeacherId == teacherId && (beginDate < oto.EndDate || oto.EndDate == null)
                                             select new 
                                             {
                                                 cs.CourseScheduleId,
                                                 cs.DayOfWeek,
                                                 cs.CourseInstanceId,
                                                 cs.GroupCourseInstanceId,
                                                 cs.BeginTime,
                                                 cs.EndTime,
                                                 l.LearnerId,
                                                 LearnerFirstName = l.FirstName,
                                                 LearnerLastName = l.LastName,
                                                 oto.OrgId,
                                                 o.OrgName,
                                             }).ToListAsync();
                arrangedGroupSchedule = await (from g in _ablemusicContext.GroupCourseInstance
                                             join cs in _ablemusicContext.CourseSchedule on g.GroupCourseInstanceId equals cs.GroupCourseInstanceId
                                             join o in _ablemusicContext.Org on g.OrgId equals o.OrgId
                                             where g.TeacherId == teacherId && (beginDate < g.EndDate || g.EndDate == null)
                                             select new 
                                             {
                                                 cs.CourseScheduleId,
                                                 cs.DayOfWeek,
                                                 cs.CourseInstanceId,
                                                 cs.GroupCourseInstanceId,
                                                 cs.BeginTime,
                                                 cs.EndTime,
                                                 LearnerName = "Group",
                                                 g.OrgId,
                                                 o.OrgName,
                                             }).ToListAsync();
                amendments = await (from a in _ablemusicContext.Amendment
                                    join oto in _ablemusicContext.One2oneCourseInstance on a.CourseInstanceId equals oto.CourseInstanceId
                                    join l in _ablemusicContext.Learner on a.LearnerId equals l.LearnerId
                                    join cs in _ablemusicContext.CourseSchedule on a.CourseScheduleId equals cs.CourseScheduleId
                                    //where oto.TeacherId == teacherId
                                    select new Amendment
                                    {
                                        CourseInstanceId = a.CourseInstanceId,
                                        AmendmentId = a.AmendmentId,
                                        OrgId = a.OrgId,
                                        DayOfWeek = a.DayOfWeek,
                                        BeginTime = cs.BeginTime,
                                        EndTime = cs.EndTime,
                                        LearnerId = a.LearnerId,
                                        RoomId = a.RoomId,
                                        BeginDate = a.BeginDate,
                                        EndDate = a.EndDate,
                                        CreatedAt = a.CreatedAt,
                                        Reason = a.Reason,
                                        IsTemporary = a.IsTemporary,
                                        AmendType = a.AmendType,
                                        CourseScheduleId = a.CourseScheduleId,
                                        Learner = a.Learner,
                                        TeacherId = oto.TeacherId,
                                    }).ToListAsync();
                arrangedLessons = await _ablemusicContext.Lesson.Where(l => l.TeacherId == teacherId && l.BeginTime.HasValue &&
                l.BeginTime.Value.Date >= beginDate.Date && l.IsCanceled != 1).Include(l => l.Learner).Include(l => l.Org).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                // return BadRequest(result);
                return result;
            }
            if (teacher == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "Teacher id not found";
                // return NotFound(result);
                return result;
            }

            customisedAvailableDays = new List<Object>();

            foreach (var ad in availableDays)
            {
                var repeatDate = false;
                foreach(var cad in customisedAvailableDays)
                {
                    if(cad.DayOfWeek == ad.DayOfWeek)
                    {
                        cad.Orgs.Add(new 
                        {
                            ad.Org.OrgId,
                            ad.Org.OrgName
                        });
                        repeatDate = true;
                    }
                }
                if (!repeatDate)
                {
                    customisedAvailableDays.Add(new
                    {
                        ad.DayOfWeek,
                        Orgs = new List<Object>()
                        {
                            new 
                            {
                                 ad.Org.OrgId,
                                 ad.Org.OrgName,
                            }
                        },
                    });
                }              
            }

            customisedArranged = new List<Object>();
            foreach(var oto in arrangedOtOSchedule)
            {
                customisedArranged.Add(new
                {
                    oto.DayOfWeek,
                    TimeBegin = oto.BeginTime,
                    TimeEnd = oto.EndTime,
                    LearnerName = oto.LearnerFirstName + " " + oto.LearnerLastName,
                    oto.CourseScheduleId,
                    oto.OrgId,
                    oto.OrgName,
                });
            }
            foreach(var g in arrangedGroupSchedule)
            {
                customisedArranged.Add(new
                {
                    g.DayOfWeek,
                    TimeBegin = g.BeginTime,
                    TimeEnd = g.EndTime,
                    g.LearnerName,
                    g.CourseScheduleId,
                    g.OrgId,
                    g.OrgName,
                });
            }
            foreach(var lesson in arrangedLessons)
            {
                customisedArranged.Add(new
                {
                    DayOfWeek = lesson.BeginTime.Value.ToDayOfWeek(),
                    TimeBegin = lesson.BeginTime.Value,
                    TimeEnd = lesson.EndTime.Value,
                    LearnerName = lesson.Learner == null ? null : lesson.Learner.FirstName + " " + lesson.Learner.LastName,
                    CourseScheduleId = -1,
                    lesson.OrgId,
                    lesson.Org?.OrgName
                });
            }

            var dayoffsOverThreeMonth = amendments.Where(a => a.TeacherId == teacherId && a.AmendType == 1 && beginDate.Date >= a.BeginDate.Value.Date && beginDate.Date <= a.EndDate.Value.Date &&
            a.EndDate.Value.Month - a.BeginDate.Value.Month >= 3).ToList();
            var changeTemporarily = amendments.Where(a => a.TeacherId == teacherId && a.AmendType == 2 && a.IsTemporary == 1 && beginDate < a.EndDate).ToList();
            var changePermanentlyExpired = amendments.Where(a => a.TeacherId == teacherId && a.AmendType == 2 && a.IsTemporary == 0 && beginDate > a.BeginDate).ToList();
            var changePermanentlyNotExpired = amendments.Where(a => a.TeacherId == teacherId && a.AmendType == 2 && a.IsTemporary == 0 && beginDate <= a.BeginDate).ToList();

            if(dayoffsOverThreeMonth != null)
            {
                foreach(var d in dayoffsOverThreeMonth)
                {
                    List<Amendment> checkLearnerMadeChangeBeforeDayOff = amendments.Where(a => a.LearnerId == d.LearnerId && a.AmendType == 2 && a.IsTemporary == 0 && a.CreatedAt < d.CreatedAt).OrderBy(a => a.CreatedAt).ToList();
                    TimeSpan beginTime = d.BeginTime.Value;
                    TimeSpan endTime = d.EndTime.Value;
                    foreach(var changedBeforeDayOff in checkLearnerMadeChangeBeforeDayOff)
                    {
                        beginTime = changedBeforeDayOff.BeginTime.Value;
                        endTime = changedBeforeDayOff.EndTime.Value;
                    }

                    customisedDayoff.Add(new
                    {
                        d.DayOfWeek,
                        TimeBegin = beginTime,
                        TimeEnd = endTime,
                        LearnerName = d.Learner.FirstName + " " + d.Learner.LastName,
                        d.CourseScheduleId,
                    });
                    customisedArranged.Remove(customisedArranged.Find(c => c.CourseScheduleId == d.CourseScheduleId));
                }
            }

            if(changeTemporarily != null)
            {
                foreach(var ct in changeTemporarily)
                {
                    customisedTempChange.Add(new
                    {
                        ct.DayOfWeek,
                        TimeBegin = ct.BeginTime,
                        TimeEnd = ct.EndTime,
                        LearnerName = ct.Learner.FirstName + " " + ct.Learner.LastName,
                        ct.CourseScheduleId,
                    });
                }
            }

            if(changePermanentlyExpired != null)
            {
                foreach(var cpe in changePermanentlyExpired)
                {
                    customisedArranged.Remove(customisedArranged.Find(c => c.CourseScheduleId == cpe.CourseScheduleId));
                    customisedArranged.Add(new
                    {
                        cpe.DayOfWeek,
                        TimeBegin = cpe.BeginTime,
                        TimeEnd = cpe.EndTime,
                        LearnerName = cpe.Learner.FirstName + " " + cpe.Learner.LastName,
                        cpe.CourseScheduleId,
                    });
                }
            }

            if(changePermanentlyNotExpired != null)
            {
                foreach(var cpne in changePermanentlyNotExpired)
                {
                    customisedTempChange.Add(customisedArranged.Find(c => c.CourseScheduleId == cpne.CourseScheduleId));
                    customisedArranged.Remove(customisedArranged.Find(c => c.CourseScheduleId == cpne.CourseScheduleId));
                    customisedArranged.Add(new
                    {
                        cpne.DayOfWeek,
                        TimeBegin = cpne.BeginTime,
                        TimeEnd = cpne.EndTime,
                        LearnerName = cpne.Learner.FirstName + " " + cpne.Learner.LastName,
                        cpne.CourseScheduleId,
                    });
                }
            }

            result.Data = new
            {
                AvailableDay = customisedAvailableDays,
                Arranged = customisedArranged,
                Dayoff = customisedDayoff.Where(d => d.TimeBegin != null),
                TempChange = customisedTempChange,
            };

            // return Ok(result);
            return result;
        }
    }
}
