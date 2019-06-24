using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;

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
        [HttpGet("{teacherId}/{StartDate}")]
        public async Task<IActionResult> Get(int teacherId, string StartDate)
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
                                             where oto.TeacherId == teacherId && beginDate < oto.EndDate 
                                             select new 
                                             {
                                                 CourseScheduleId = cs.CourseScheduleId,
                                                 DayOfWeek = cs.DayOfWeek,
                                                 CourseInstanceId = cs.CourseInstanceId,
                                                 GroupCourseInstanceId = cs.GroupCourseInstanceId,
                                                 BeginTime = cs.BeginTime,
                                                 EndTime = cs.EndTime,
                                                 LearnerId = l.LearnerId,
                                                 LearnerFirstName = l.FirstName,
                                                 LearnerLastName = l.LastName,
                                                 OrgId = oto.OrgId,
                                                 OrgName = o.OrgName,
                                             }).ToListAsync();
                arrangedGroupSchedule = await (from g in _ablemusicContext.GroupCourseInstance
                                             join cs in _ablemusicContext.CourseSchedule on g.GroupCourseInstanceId equals cs.GroupCourseInstanceId
                                             join o in _ablemusicContext.Org on g.OrgId equals o.OrgId
                                             where g.TeacherId == teacherId && beginDate < g.EndDate
                                             select new 
                                             {
                                                 CourseScheduleId = cs.CourseScheduleId,
                                                 DayOfWeek = cs.DayOfWeek,
                                                 CourseInstanceId = cs.CourseInstanceId,
                                                 GroupCourseInstanceId = cs.GroupCourseInstanceId,
                                                 BeginTime = cs.BeginTime,
                                                 EndTime = cs.EndTime,
                                                 LearnerName = "Group",
                                                 OrgId = g.OrgId,
                                                 OrgName = o.OrgName,
                                             }).ToListAsync();
                amendments = await (from oto in _ablemusicContext.One2oneCourseInstance
                                    join a in _ablemusicContext.Amendment on oto.CourseInstanceId equals a.CourseInstanceId
                                    join l in _ablemusicContext.Learner on a.LearnerId equals l.LearnerId
                                    where oto.TeacherId == teacherId
                                    select new Amendment
                                    {
                                        CourseInstanceId = a.CourseInstanceId,
                                        AmendmentId = a.AmendmentId,
                                        OrgId = a.OrgId,
                                        DayOfWeek = a.DayOfWeek,
                                        BeginTime = a.BeginTime,
                                        EndTime = a.EndTime,
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
                                    }).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            if (teacher == null)
            {
                result.IsSuccess = false;
                result.IsFound = false;
                result.ErrorMessage = "Teacher id not found";
                return NotFound(result);
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
                            OrgId = ad.Org.OrgId,
                            OrgName = ad.Org.OrgName
                        });
                        repeatDate = true;
                    }
                }
                if (!repeatDate)
                {
                    customisedAvailableDays.Add(new
                    {
                        DayOfWeek = ad.DayOfWeek,
                        Orgs = new List<Object>()
                        {
                            new 
                            {
                                OrgId = ad.Org.OrgId,
                                OrgName = ad.Org.OrgName,
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
                    DayOfWeek = oto.DayOfWeek,
                    TimeBegin = oto.BeginTime,
                    TimeEnd = oto.EndTime,
                    LearnerName = oto.LearnerFirstName + " " + oto.LearnerLastName,
                    CourseScheduleId = oto.CourseScheduleId,
                    OrgId = oto.OrgId,
                    OrgName = oto.OrgName,
                });
            }
            foreach(var g in arrangedGroupSchedule)
            {
                customisedArranged.Add(new
                {
                    DayOfWeek = g.DayOfWeek,
                    TimeBegin = g.BeginTime,
                    TimeEnd = g.EndTime,
                    LearnerName = g.LearnerName,
                    CourseScheduleId = g.CourseScheduleId,
                    OrgId = g.OrgId,
                    OrgName = g.OrgName,
                });
            }

            var dayoffsOverThreeMonth = amendments.Where(a => a.AmendType == 1 && beginDate > a.BeginDate && beginDate < a.EndDate &&
            a.EndDate.Value.Month - a.BeginDate.Value.Month >= 3).ToList();
            var changeTemporarily = amendments.Where(a => a.AmendType == 2 && a.IsTemporary == 1 && beginDate < a.EndDate).ToList();
            var changePermanentlyExpired = amendments.Where(a => a.AmendType == 2 && a.IsTemporary == 0 && beginDate > a.BeginDate).ToList();
            var changePermanentlyNotExpired = amendments.Where(a => a.AmendType == 2 && a.IsTemporary == 0 && beginDate <= a.BeginDate).ToList();

            if(dayoffsOverThreeMonth != null)
            {
                foreach(var d in dayoffsOverThreeMonth)
                {
                    customisedDayoff.Add(new
                    {
                        DayOfWeek = d.DayOfWeek,
                        TimeBegin = d.BeginTime,
                        TimeEnd = d.EndTime,
                        LearnerName = d.Learner.FirstName + " " + d.Learner.LastName,
                        CourseScheduleId = d.CourseScheduleId,
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
                        DayOfWeek = ct.DayOfWeek,
                        TimeBegin = ct.BeginTime,
                        TimeEnd = ct.EndTime,
                        LearnerName = ct.Learner.FirstName + " " + ct.Learner.LastName,
                        CourseScheduleId = ct.CourseScheduleId,
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
                        DayOfWeek = cpe.DayOfWeek,
                        TimeBegin = cpe.BeginTime,
                        TimeEnd = cpe.EndTime,
                        LearnerName = cpe.Learner.FirstName + " " + cpe.Learner.LastName,
                        CourseScheduleId = cpe.CourseScheduleId,
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
                        DayOfWeek = cpne.DayOfWeek,
                        TimeBegin = cpne.BeginTime,
                        TimeEnd = cpne.EndTime,
                        LearnerName = cpne.Learner.FirstName + " " + cpne.Learner.LastName,
                        CourseScheduleId = cpne.CourseScheduleId,
                    });
                }
            }

            result.Data = new
            {
                AvailableDay = customisedAvailableDays,
                Arranged = customisedArranged,
                Dayoff = customisedDayoff,
                TempChange = customisedTempChange,
            };

            return Ok(result);
        }
    }
}
