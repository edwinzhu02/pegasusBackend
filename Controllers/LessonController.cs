using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController: BasicController
    {

        public LessonController(ablemusicContext ablemusicContext, ILogger<LessonController> log) : base(ablemusicContext, log)
        {
        }

        [HttpGet("[action]/{userId}/{today}")]
        public async Task<IActionResult> GetMobileLessonsForTeacherbyDate(DateTime today, short userId)
        {
            var result = new Result<object>();
            try
            {
                var teacher = _ablemusicContext.Teacher.FirstOrDefault(s => s.UserId == userId);
                if (teacher == null)
                {
                    throw new Exception("You are not the teacher");
                }
                var teacherId = teacher.TeacherId;

                var lessons = _ablemusicContext.Lesson
                    .Include(s=>s.TrialCourse)
                    .Include(s=>s.Learner)
                    .Include(group => group.GroupCourseInstance)
                    .ThenInclude(learnerCourse => learnerCourse.LearnerGroupCourse)
                    .ThenInclude(learner => learner.Learner)
                    .Include(s => s.GroupCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s => s.CourseInstance)
                    .ThenInclude(w => w.Course)
                    .Where(s => s.BeginTime >= today.AddMonths(-1) && s.TeacherId == teacherId)
                    .Select(s=>new
                    {
                        s.BeginTime,s.LessonId,
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName,
                        learner = IsNull(s.GroupCourseInstance)?new List<Object>(){new {s.Learner.FirstName,s.Learner.LastName,s.Learner.LearnerId}}:null,
                        learners = IsNull(s.GroupCourseInstance)?null:s.GroupCourseInstance.LearnerGroupCourse.
                            Select(w=>new{w.Learner.FirstName,w.Learner.LastName,w.Learner.LearnerId}),
                    })
                    .ToList();
                var item = new Dictionary<string,List<object>>();
                lessons.ForEach(s =>
                {
                    var year = s.BeginTime.Value.Year.ToString();
                    var month = s.BeginTime.Value.Month.ToString().Length == 1
                        ? "0" + s.BeginTime.Value.Month.ToString()
                        : s.BeginTime.Value.Month.ToString();
                    var day = s.BeginTime.Value.Day.ToString().Length == 1
                        ? "0" + s.BeginTime.Value.Day.ToString()
                        : s.BeginTime.Value.Day.ToString();
                    var date = year + "-" + month + "-" + day;
                    var time = s.BeginTime.Value.TimeOfDay;
                    if (item.ContainsKey(date))
                    {
                        item[date].Add(new
                        {
                            name=s.CourseName,
                            info=new
                            {
                                time,s.learner,s.learners
                            }
                        });
                    }
                    else
                    {
                        item.Add(date,new List<object>{new
                        {
                            name=s.CourseName,
                            info=new
                            {
                                time,s.learner,s.learners
                            }
                        }});
                    }
                });
                
                result.Data = item;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        
        //GET: http://localhost:5000/api/lesson/GetLessonsForReceptionist/:userId/:date
        [HttpGet("[action]/{userId}/{date}")]
        //for receptionist
        public async Task<IActionResult> GetLessonsForReceptionist(int userId,DateTime date)
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _ablemusicContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var details =await  _ablemusicContext.Lesson
                    .Where(s=>s.OrgId==orgId&&s.BeginTime.Value.Year == date.Year && s.BeginTime.Value.Month == date.Month && s.BeginTime.Value.Day == date.Day)
                    .Include(s=>s.CourseInstance)
                    .Include(s=>s.Learner)
                    .Include(s => s.Teacher)
                    .Include(s=>s.Room)
                    .Include(s=>s.Org)
                    .Include(s=>s.TrialCourse)
                    .Include(group => group.GroupCourseInstance)
                    .ThenInclude(learnerCourse => learnerCourse.LearnerGroupCourse)
                    .ThenInclude(learner => learner.Learner)
                    .Include(s=>s.AwaitMakeUpLessonNewLesson)
                    .Select(s =>new {id = s.LessonId, resourceId = s.RoomId, start = s.BeginTime,end=s.EndTime,
                        title=IsNull(s.GroupCourseInstance)?IsNull(s.CourseInstance)?"T":"1":"G",description="",
                        teacher=s.Teacher.FirstName.ToString(),
                        s.IsCanceled,
                        s.IsConfirm,
                        s.IsChanged,
                        s.OrgId,
                        s.InvoiceNum,
                        newLesson= new
                        {
                            TeacherFirstName = _ablemusicContext.Teacher.FirstOrDefault(z=>z.TeacherId==_ablemusicContext.Lesson
                                                                                                  .FirstOrDefault(r=>r.LessonId==s.NewLessonId).TeacherId).FirstName,
                            TeacherLastName = _ablemusicContext.Teacher.FirstOrDefault(z=>z.TeacherId==_ablemusicContext.Lesson
                                                                                                  .FirstOrDefault(r=>r.LessonId==s.NewLessonId).TeacherId).LastName,
                            RoomName = _ablemusicContext.Room.FirstOrDefault(z=>z.RoomId==_ablemusicContext.Lesson
                                                                                   .FirstOrDefault(r=>r.LessonId==s.NewLessonId).RoomId).RoomName,
                            
                            OrgName = _ablemusicContext.Org.FirstOrDefault(z=>z.OrgId==_ablemusicContext.Lesson
                                                                                  .FirstOrDefault(r=>r.LessonId==s.NewLessonId).OrgId).OrgName,
                            BeginTime =_ablemusicContext.Lesson
                                .FirstOrDefault(r=>r.LessonId==s.NewLessonId).BeginTime,
                            EndTime = _ablemusicContext.Lesson
                                .FirstOrDefault(r=>r.LessonId==s.NewLessonId).EndTime,
                            
                            
                        },
                        s.Reason,
                        /*isReadyToOwn=IsNull(s.GroupCourseInstance)?IsNull(s.CourseInstance)?s.IsPaid==1?0:1:
                            _ablemusicContext.LessonRemain.FirstOrDefault(q=>q.LearnerId==s.LearnerId && q.CourseInstanceId==s.CourseInstanceId).Quantity<=3?
                            1:0
                            :0,*/
                        // isReadyToOwn = IsNull(s.GroupCourseInstance)?IsNull(s.CourseInstance)?s.IsPaid==1?0:1:
                        //         _ablemusicContext.LessonRemain.FirstOrDefault(q=>q.LearnerId==s.LearnerId && q.CourseInstanceId==s.CourseInstanceId)==null?
                        //         1: _ablemusicContext.LessonRemain.FirstOrDefault(q=>q.LearnerId==s.LearnerId && q.CourseInstanceId==s.CourseInstanceId).Quantity<=3?
                        //             1:0
                        //     :0,
                        IsPaid = s.IsPaid,IsTrial = s.IsTrial,
                        learner = IsNull(s.GroupCourseInstance)?new List<Object>(){new {s.Learner.FirstName,s.Learner.LearnerId}}:null,
                        learners = IsNull(s.GroupCourseInstance)?null:s.GroupCourseInstance.LearnerGroupCourse.
                                Select(w=>new{w.Learner.FirstName,w.Learner.LearnerId}),
                        IsGroup=!IsNull(s.GroupCourseInstance),
                        info = new
                        {
                            s.Room.RoomName,s.RoomId,s.Org.OrgName,s.OrgId,s.TeacherId,TeacherLastName=s.Teacher.LastName,TeacherFirstName=s.Teacher.FirstName,s.BeginTime,s.EndTime,
                            CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName,
                            s.LessonId,courseId=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseId:IsNull(s.CourseInstance)?s.TrialCourseId:s.CourseInstance.Course.CourseId,s.LearnerId
                        }
                    }).ToListAsync();
                //var preDetails =new List<object>() ;
                // foreach(var ele in details){
                //     ele.isReadyToOwn = 1;
                // }
                
                var preDetail = details.Select(e=>new {e.id,e.resourceId,e.start,e.end,e.title,
                            e.description,e.teacher,e.IsCanceled,e.IsConfirm,
                            e.IsChanged,e.OrgId,e.InvoiceNum,
                            e.newLesson,e.Reason,e.learner,e.learners,e.IsGroup,e.info,
                            isReadyToOwn=e.IsTrial==1?(e.IsPaid==1?0:1):isReadyOwe(e.InvoiceNum)});
                result.Data = preDetail;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);

        }
        
        

        
        [HttpGet("[action]/{teacherId}/{beginDate}")]
        public async Task<IActionResult> GetLessonsForTeacher(short teacherId, DateTime beginDate)
        {
            Result<Object> result = new Result<object>();
            try
            {
                var endDate = beginDate.AddDays(6);
                var details = _ablemusicContext.Lesson.Where(s => s.TeacherId == teacherId)
                    .Where(s=>beginDate.Date <= s.EndTime.Value.Date && s.EndTime.Value.Date <= endDate.Date)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .ThenInclude(s=>s.AvailableDays)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        title=IsNull(q.GroupCourseInstance)?IsNull(q.CourseInstance)?"T":"1":"G",start=q.BeginTime,end=q.EndTime,
                        student=IsNull(q.GroupCourseInstance)?IsNull(q.CourseInstance)?new List<string>{q.Learner.FirstName}:new List<string>{q.Learner.FirstName}:q.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        description="", courseName=!IsNull(q.GroupCourseInstance)?q.GroupCourseInstance.Course.CourseName:IsNull(q.CourseInstance)?q.TrialCourse.CourseName:q.CourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName, orgAbbr = q.Org.Abbr,
                        q.OrgId,q.RoomId,
                        q.IsConfirm,q.IsChanged,q.IsCanceled,q.Reason,q.BeginTime, q.LessonId, q.LearnerId,
                        newLesson= new
                        {
                            RoomName = _ablemusicContext.Room.FirstOrDefault(z=>z.RoomId==_ablemusicContext.Lesson
                                                                                    .FirstOrDefault(r=>r.LessonId==q.NewLessonId).RoomId).RoomName,
                            
                            OrgName = _ablemusicContext.Org.FirstOrDefault(z=>z.OrgId==_ablemusicContext.Lesson
                                                                                  .FirstOrDefault(r=>r.LessonId==q.NewLessonId).OrgId).OrgName,
                            BeginTime =_ablemusicContext.Lesson
                                .FirstOrDefault(r=>r.LessonId==q.NewLessonId).BeginTime,
                            
                        }
                        
                    });
                result.Data = await details.ToListAsync();

                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }
        

        [HttpGet("[action]/{teacherId}")]
        public async Task<IActionResult> GetLessonsTeacherId(short teacherId)
        {
            Result<Object> result = new Result<object>();
            try
            {
               
                var details = _ablemusicContext.Lesson.Where(s => s.TeacherId == teacherId)
                    .Where(s=>s.IsCanceled ==0)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        title=IsNull(q.GroupCourseInstanceId)?"1":"G",start=q.BeginTime,end=q.EndTime,
                        student=IsNull(q.GroupCourseInstance)?new List<string>{q.Learner.FirstName}:q.GroupCourseInstance.LearnerGroupCourse.Select(w=>w.Learner.FirstName),
                        description="", courseName=IsNull(q.GroupCourseInstanceId)?q.CourseInstance.Course.CourseName:q.GroupCourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName
                    });
                result.Data = await details.ToListAsync();

                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpGet("[action]/{learnerId}")]
        public async Task<IActionResult> GetArrangedLessonsByLearner(int learnerId)
        {
            Result<Object> result = new Result<object>();
            try
            {
               
                var details = _ablemusicContext.Lesson.Where(s => s.LearnerId == learnerId)
                    .Where(s=>s.IsCanceled ==0 &&s.IsConfirm !=1)
                    .Include(s=>s.Room)
                    .Include(s=>s.Learner)
                    .Include(s=>s.Teacher)
                    .Include(s=>s.GroupCourseInstance)
                    .Include(s=>s.CourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.GroupCourseInstance).ThenInclude(w=>w.Course)
                    .Include(s=>s.Org)
                    .Select(q=>new
                    {
                        start=q.BeginTime,end=q.EndTime,
                        learnerFirstName=q.Learner.FirstName,learnerLastName=q.Learner.LastName,
                        teacherFirstName=q.Teacher.FirstName,TeacherLastName=q.Teacher.LastName,
                        courseName=IsNull(q.GroupCourseInstanceId)?q.CourseInstance.Course.CourseName:q.GroupCourseInstance.Course.CourseName,
                        orgName= q.Org.OrgName, roomName=q.Room.RoomName
                    });
                result.Data = await details.ToListAsync();
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return Ok(result);
        }
                
        [HttpGet("[action]/{userId}/{beginDate}/{endDate}")]

        public async Task<IActionResult> GetLessonsBetweenDate(DateTime? beginDate, DateTime? endDate, int? userId)
        {
            Result<Object> result = new Result<Object>();
            try
            {
                var staff = _ablemusicContext.Staff.FirstOrDefault(s => s.UserId == userId);
                var orgId = _ablemusicContext.StaffOrg.FirstOrDefault(s => s.StaffId == staff.StaffId).OrgId;
                var items = _ablemusicContext.Lesson
                    .Include(s => s.Teacher)
                    .Include(s => s.Learner)
                    .Include(s => s.Room)
                    .Include(s => s.Org)
                    .Include(s => s.GroupCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s => s.CourseInstance)
                    .ThenInclude(w => w.Course)
                    .Where(s => beginDate.Value.Date <= s.EndTime.Value.Date &&
                                s.EndTime.Value.Date <= endDate.Value.Date && s.OrgId == orgId)
                    .Select(s => new
                    {
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName,
                        TeacherFirstName = s.Teacher.FirstName, s.BeginTime, s.EndTime, s.LessonId,
                        Room = s.Room.RoomName, Branch = s.Org.OrgName, s.IsCanceled, CancelReson = s.Reason,
                        s.IsConfirm,
                        s.IsTrial, Learner = s.Learner.FirstName, Learners = "", s.LearnerId, s.RoomId, s.TeacherId,
                        s.OrgId,
                        courseId=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseId:IsNull(s.CourseInstance)?s.TrialCourseId:s.CourseInstance.Course.CourseId
                    }).OrderByDescending(s=>s.BeginTime);
                
                result.Data = await items.ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        [HttpGet("[action]/{learnerId}")]

        public async Task<IActionResult> GetLessonsForLearner(int? learnerId)
        {
            Result<Object> result = new Result<Object>();
            try
            {   
                var termEndDate = await _ablemusicContext.Term.Where(t => t.EndDate>DateTime.UtcNow.AddDays(21)).OrderBy(t =>t.EndDate).
                    Select(t => t.EndDate).FirstOrDefaultAsync();
                dynamic  items = await _ablemusicContext.Lesson
                    .Include(s => s.Teacher)
                    .Include(s => s.Learner)
                    .Include(s => s.Room)
                    .Include(s => s.Org)
                    .Include(s => s.GroupCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.LearnerGroupCourse)
                    .Include(s => s.CourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s => s.NewLesson)
                    .ThenInclude(s => s.Teacher)                    
                    .Include(s => s.NewLesson)
                    .ThenInclude(s => s.Org)                    
                    .Include(s => s.NewLesson)
                    .ThenInclude(s => s.Room)                    
                    .Where(s => (s.LearnerId ==learnerId 
                    || s.GroupCourseInstance.LearnerGroupCourse.ToList().Exists(e=>e.LearnerId == learnerId) )
                     && s.BeginTime <termEndDate)
                    .Select(s => new
                    {
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName,
                        TeacherFirstName = s.Teacher.FirstName, s.BeginTime, s.EndTime, s.LessonId,
                        Room = s.Room.RoomName, Branch = s.Org.OrgName,BranchAbbr = s.Org.Abbr, s.IsCanceled, CancelReson = s.Reason,
                        s.IsConfirm,
                        s.IsTrial, Learner = s.Learner.FirstName, Learners = "", s.LearnerId, s.RoomId, s.TeacherId,
                        s.OrgId,
                        courseId=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseId:IsNull(s.CourseInstance)?s.TrialCourseId:s.CourseInstance.Course.CourseId,
                        IsChanged = s.IsChanged
                        ,newLessons =new {
                            TeacherFirstName = s.NewLesson.Teacher.FirstName,
                            BranchAbbr = s.NewLesson.Org.Abbr,
                            Room = s.NewLesson.Room.RoomName,
                            BeginTime = s.NewLesson.BeginTime,
                            EndTime = s.NewLesson.EndTime
                        }
                        ,IsPaid =s.IsTrial==1?s.IsPaid+0:
                            _ablemusicContext.Invoice.
                            Where(i => i.InvoiceNum==s.InvoiceNum
                        && i.IsActive==1 && i.InvoiceNum != null)
                            .Select(i => i.IsPaid+0).FirstOrDefault()??((short)0)
                    }).ToListAsync();
                

                result.Data = items;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        private short isReadyOwe(string invoiceNum){
            var invoice = _ablemusicContext.Invoice.
                Where(i => i.InvoiceNum  == invoiceNum 
                    && i.IsActive==1).FirstOrDefault();

            if (invoice == null) return 1;
            if (invoice.IsPaid == 0) return 1;
            if (invoice.CourseInstanceId==null) return 0;
            
             var unpaidInvoice = _ablemusicContext.Invoice.
                Where(i => i.LearnerId == invoice.LearnerId 
                    && i.IsActive==1 && i.IsPaid==0
                    && i.DueDate<toNZTimezone( DateTime.UtcNow) ).FirstOrDefault();
            if (unpaidInvoice !=null ) return 1;
            return 0;
        }
        private static bool FindGroup(Lesson s,int? learnerId){
            if (s.GroupCourseInstance == null)
            {
                return false;
            }
            for (var i = 0; i < s.GroupCourseInstance.LearnerGroupCourse.Count; i++)
            {
                if (s.GroupCourseInstance.LearnerGroupCourse.ToList()[i].LearnerId == learnerId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}