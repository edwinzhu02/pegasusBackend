using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;


namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnetoOneCourseInstanceController : BasicController

    {
        private readonly IMapper _mapper;
        private readonly LessonGenerateService _lessonGenerateService;
        private readonly IInvoicePatchService _invoicePatchService;

        public OnetoOneCourseInstanceController(ablemusicContext ablemusicContext, ILogger<OnetoOneCourseInstanceController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
            _lessonGenerateService = new LessonGenerateService(_ablemusicContext, _mapper);
            _invoicePatchService = new InvoicePatchService(_ablemusicContext);
        }
        // [HttpGet("getbyteacher/{teacherId}")]
        // public async Task<ActionResult> GetCourses(short teacherId)
        // {
        //     Result<Object> result = new Result<Object>();
           
        //     var item =_ablemusicContext.One2oneCourseInstance
        //         .Include(o =>o.Learner)
        //         .Include(o =>o.Course)
        //         .Include(o =>o.CourseSchedule)
        //         .Where(s => s.TeacherId== teacherId && 
        //         (s.EndDate ==null || s.EndDate>new DateTime()));
        //      result.Data = item;
        //      return Ok(result);
        // }
        [HttpPut("{instanceId}/{endDate}")]
        public async Task<IActionResult> UpdateOnetoOneCourseInstance(int instanceId, DateTime endDate)
        {
            var result =quitCourse(instanceId,endDate);
            if (result.IsSuccess==false)
                return BadRequest(result);
            else
                return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddOnetoOneCourseInstance([FromBody] OnetoOneCourseInstancesModel model)
        {
            var result = addCourse(model);
            if (result.IsSuccess==false)
                 return BadRequest(result);
            else
                return Ok(result);
        }
        // [HttpPut]
        [HttpPut("[action]/{lessonId}/{duration}")]
        public async Task<IActionResult> ChangeOnetoOneCourseInstance(int lessonId,short duration)
        {
            var result = new Result<dynamic>();
            OnetoOneCourseInstancesModel model = new OnetoOneCourseInstancesModel();

            try {
                var lesson =  _ablemusicContext.Lesson.
                            Where(l => l.LessonId ==lessonId)
                        .FirstOrDefault();
                var courseInstance =  _ablemusicContext.One2oneCourseInstance.Include(c =>c.CourseSchedule).
                                    Where(l => l.CourseInstanceId ==lesson.CourseInstanceId)
                                    .FirstOrDefault();
                var orginalCourse =  _ablemusicContext.Course.
                                    Where(l => l.CourseId ==courseInstance.CourseId)
                                    .FirstOrDefault();

                var newCourse =  _ablemusicContext.Course.
                                    Where(l => l.CourseType==orginalCourse.CourseType &&
                                    l.Level==orginalCourse.Level &&
                                    l.TeacherLevel==orginalCourse.TeacherLevel &&
                                    l.CourseCategoryId==orginalCourse.CourseCategoryId &&
                                    l.Duration==duration)
                                    .FirstOrDefault();

                if (newCourse ==null ||courseInstance ==null ||
                    orginalCourse ==null || lesson==null){
                        result.IsSuccess=false;
                        result.ErrorMessage = "This lesson can not be changed!";
                        return BadRequest(result);
                    }
                model.OnetoOneCourses = new List<OnetoOneCourseInstanceModel>();
                model.OnetoOneCourses.Add(
                    new OnetoOneCourseInstanceModel
                    {
                        id = courseInstance.CourseInstanceId,
                        CourseId =newCourse.CourseId,
                        TeacherId  =courseInstance.TeacherId,
                        OrgId  =courseInstance.OrgId,
                        RoomId  =courseInstance.RoomId,
                        BeginDate  = lesson.BeginTime,
                        EndDate  =courseInstance.EndDate,
                        LearnerId  =courseInstance.LearnerId,
                        Schedule =  new CourseScheduleModel
                            {
                                DayOfWeek = (byte) DayofWeekToInt(lesson.BeginTime.Value.DayOfWeek),
                                BeginTime = lesson.BeginTime.Value.TimeOfDay
                            }
                    }   
                );
            var resultQuit  = quitCourse(courseInstance.CourseInstanceId, lesson.BeginTime.Value.AddDays(-1));

            if (resultQuit.IsSuccess==false)
                 return BadRequest(resultQuit);


            var resultAdd  = addCourse(model);
            if (resultAdd.IsSuccess==false)
                 return BadRequest(resultAdd);
            else
                return Ok(resultAdd);

            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
            
        }        
        private bool isNewStudent(int? learnerId)
        {
            DateTime dateNow = DateTime.UtcNow.ToNZTimezone();
            DateTime? enrollDate =  _ablemusicContext.Learner.Where(l => l.LearnerId ==learnerId)
                                .Select(l =>l.EnrollDate).FirstOrDefault();
            if (enrollDate==null) return false;
            if  ((dateNow - enrollDate.Value ).TotalDays <7*2) return true;                 
            return false;
        }
        private  Result<string> addCourse(OnetoOneCourseInstancesModel model)
        {
            var result = new Result<string>();
            List<int> courseInstanceIds = new List<int>();
            int? learnerId = model.OnetoOneCourses[0].LearnerId;

            One2oneCourseInstance one2oneCourseInstance =  new One2oneCourseInstance();
            try
            {
                using (var dbtransaction =_ablemusicContext.Database.BeginTransaction())
                {
                    model.OnetoOneCourses.ForEach(s =>
                    {
                        
                        var room = _ablemusicContext.AvailableDays.FirstOrDefault(
                            q => q.TeacherId == s.TeacherId && q.OrgId == s.OrgId &&
                                 q.DayOfWeek == s.Schedule.DayOfWeek);
                            
                        short roomId;
                        if (room == null)
                        {
                            throw new Exception("This teacher is not available!");
                        }
                        if (s.RoomId != null && s.RoomId != 0)
                        {   
                            roomId = s.RoomId.Value;
                        }
                        else if (room.RoomId != null){
                            roomId = room.RoomId.Value;
                        }
                        else{
                            throw new Exception("This teacher has no room in this branch!");
                        }
                                              
                        var durationType = _ablemusicContext.Course.FirstOrDefault(w => w.CourseId == s.CourseId).Duration;
                        one2oneCourseInstance = new One2oneCourseInstance
                        {
                            CourseId = s.CourseId,TeacherId = s.TeacherId, OrgId = s.OrgId,
                            BeginDate = s.BeginDate, EndDate = s.EndDate, LearnerId = s.LearnerId,
                            RoomId = roomId,
                            CourseSchedule = new List<CourseSchedule>()
                            {
                                new CourseSchedule
                                {
                                    DayOfWeek = s.Schedule.DayOfWeek,
                                    BeginTime = s.Schedule.BeginTime, 
                                    EndTime = GetEndTimeForOnetoOneCourseSchedule(s.Schedule.BeginTime,durationType)
                                }
                            }
                        };
                        _ablemusicContext.Add(one2oneCourseInstance);
                    });
                    _ablemusicContext.SaveChangesAsync();
                    courseInstanceIds.Add(one2oneCourseInstance.CourseInstanceId);                    
 
                    var newModel = _ablemusicContext.One2oneCourseInstance.
                            Where(o => o.LearnerId==model.OnetoOneCourses[0].LearnerId).
                            Select(o => new {o.BeginDate ,id= o.CourseInstanceId}).ToList();

                    newModel.ForEach(async s =>
                    {
                        await _lessonGenerateService.GetTerm((DateTime)s.BeginDate, s.id, 1);
                    });
                    if (isNewStudent(learnerId)){
                        // if (!_invoicePatchService.InvoicePatch(courseInstanceIds)) 
                        //throw new Exception("Invoice save error!");
                        _invoicePatchService.InvoicePatch(courseInstanceIds);
                    }
                    
                    _ablemusicContext.SaveChangesAsync();
                    dbtransaction.Commit();
                }

                result.Data = "success";
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }

        }
        private  Result<string> quitCourse(int instanceId, DateTime endDate)
        {
            var result = new Result<string>();
            try
            {
                var item = _ablemusicContext.One2oneCourseInstance.FirstOrDefault(s => s.CourseInstanceId == instanceId);
                decimal credit=0;

                if (item == null)
                {
                    throw new Exception("The Booking Course wasn't found");
                }
                var learner = _ablemusicContext.Learner.FirstOrDefault(c =>c.LearnerId ==item.LearnerId );
                if (learner == null)
                {
                    throw new Exception("This student wasn't found");
                }                
                item.EndDate = endDate;
                _ablemusicContext.Update(item);
                var lessons = _ablemusicContext.Lesson.Where(l =>l.BeginTime>endDate &&
                                l.CourseInstanceId==instanceId &&
                                l.IsCanceled !=1 );
                
                lessons.ForEachAsync(lesson =>{
                    lesson.IsCanceled = 1;
                    decimal originalFee = 0;
                     var invoice = _ablemusicContext.Invoice.
                     FirstOrDefault(c =>c.InvoiceNum ==lesson.InvoiceNum && c.IsActive==1); 
                     if (invoice!=null ){
                        originalFee = invoice.TotalFee.Value;
                        invoice.TotalFee -=invoice.LessonFee/invoice.LessonQuantity;
                        invoice.OwingFee -=invoice.LessonFee/invoice.LessonQuantity;
                        invoice.LessonFee -=invoice.LessonFee/invoice.LessonQuantity;                        
                        invoice.LessonQuantity --;


                        if (invoice.LessonFee==0){
                            invoice.NoteFee=0;
                            invoice.ConcertFee=0;
                            invoice.Other1Fee=0;
                            invoice.Other2Fee=0;   
                            invoice.Other3Fee=0; 
                            invoice.Other4Fee=0;
                            invoice.Other5Fee=0;   
                            invoice.Other6Fee=0; 
                            invoice.Other7Fee=0;
                            invoice.Other8Fee=0;   
                            invoice.Other9Fee=0; 
                            invoice.Other10Fee=0;
                            invoice.Other11Fee=0;   
                            invoice.Other12Fee=0;
                            invoice.Other13Fee=0;
                            invoice.Other14Fee=0;   
                            invoice.Other15Fee=0; 
                            invoice.Other16Fee=0;
                            invoice.Other17Fee=0;   
                            invoice.Other18Fee=0;                                                                                                                                             
                            invoice.TotalFee =0;
                            invoice.OwingFee =0;                                                   
                        }
                         if (invoice.IsPaid==1){
                            credit += (originalFee - invoice.TotalFee.Value);
                        }
                        _ablemusicContext.Update(invoice);
                    }
                     var invoiceWaiting = _ablemusicContext.InvoiceWaitingConfirm.
                     FirstOrDefault(c =>c.InvoiceNum ==lesson.InvoiceNum && c.IsActivate==1);
                     invoiceWaiting.TotalFee -=invoiceWaiting.LessonFee/invoiceWaiting.LessonQuantity;
                     invoiceWaiting.OwingFee -=invoiceWaiting.LessonFee/invoiceWaiting.LessonQuantity;
                     invoiceWaiting.LessonFee -=invoiceWaiting.LessonFee/invoiceWaiting.LessonQuantity;
                     invoiceWaiting.LessonQuantity --;
                    if (invoiceWaiting.LessonFee==0){
                            invoiceWaiting.NoteFee=0;
                            invoiceWaiting.ConcertFee=0;
                            invoiceWaiting.Other1Fee=0;
                            invoiceWaiting.Other2Fee=0;   
                            invoiceWaiting.Other3Fee=0; 
                            invoiceWaiting.Other4Fee=0;
                            invoiceWaiting.Other5Fee=0;   
                            invoiceWaiting.Other6Fee=0; 
                            invoiceWaiting.Other7Fee=0;
                            invoiceWaiting.Other8Fee=0;   
                            invoiceWaiting.Other9Fee=0; 
                            invoiceWaiting.Other10Fee=0;
                            invoiceWaiting.Other11Fee=0;   
                            invoiceWaiting.Other12Fee=0;    
                            invoiceWaiting.Other13Fee=0;
                            invoiceWaiting.Other14Fee=0;   
                            invoiceWaiting.Other15Fee=0;  
                            invoiceWaiting.Other16Fee=0;
                            invoiceWaiting.Other17Fee=0;   
                            invoiceWaiting.Other18Fee=0;                                                                                                                                           
                            invoiceWaiting.TotalFee =0;
                            invoiceWaiting.OwingFee =0;                                                   
                        }
                     _ablemusicContext.Update(invoiceWaiting);
                    //  if (invoiceWaiting.LessonFee==0){
                    //     _ablemusicContext.Remove(invoiceWaiting);
                    //  }
                    // //_ablemusicContext.Remove(lesson);
                });
                learner.Credit +=credit;
                _ablemusicContext.Update(learner);
                _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }

            result.Data = "success";
            return result;

        }
    }
}
