using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Pegasus_backend.ActionFilter;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsViewController : BasicController
    {
        private IMapper _mapper;
        public LessonsViewController(ablemusicContext ablemusicContext, ILogger<PaymentController> log, IMapper mapper) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        [HttpGet("{orgId}/{termId}")]
        public async Task<IActionResult> GetLessonsView(int orgId, short termId)
        {
            Result<Object> result = new Result<object>();
            List<LearnersLessonViewModel> LearnersLesson =  new List<LearnersLessonViewModel>();
            var term = await _ablemusicContext.Term.FirstOrDefaultAsync(t =>t.TermId ==termId);
            try
            {
                var courseInstances = await _ablemusicContext.One2oneCourseInstance
                    .Include(o =>o.Learner)
                    .Include(o =>o.Teacher)
                    .Include(o =>o.Course)
                    .Where(d => d.OrgId == orgId //&& d.LearnerId == 147
                    && (d.EndDate == null || d.EndDate > term.BeginDate)
                    ).ToArrayAsync();
 

                foreach (var courseInstance in courseInstances)
                {
                    var lessonView = await GetOne2OneLessons(courseInstance,term);
                    if (lessonView.Count >0) {
                    LearnersLesson.Add(
                        new LearnersLessonViewModel
                        {
                            LearnerId = courseInstance.LearnerId.Value,
                            FirstName = courseInstance.Learner.FirstName,
                            LastName = courseInstance.Learner.LastName,
                            Teacher = courseInstance.Teacher.FirstName,
                            Course = courseInstance.Course.CourseName,
                            DayOfWeek = (short) lessonView.FirstOrDefault().OriginalDate.DayOfWeek,
                            LessonsViewModel = lessonView
                        });

                    }
                }
                result.Data = LearnersLesson;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorCode = ex.Message;
                return BadRequest(result);
            }
            return Ok(result);
        }

        private async Task<List<LessonsViewModel>> GetOne2OneLessons(One2oneCourseInstance courseInstance,Term term)
        {
            List<LessonsViewModel> lessonsViewModel =  new List<LessonsViewModel>();
            try
            {
                List<Lesson> allLessons = _ablemusicContext.Lesson.Where(l => 
                        l.LearnerId==courseInstance.LearnerId
                        && l.BeginTime>term.BeginDate.Value.AddDays(-60)).ToList();

                List<AwaitMakeUpLesson> awaitMakeUpLessons = _ablemusicContext.AwaitMakeUpLesson.Where(l => 
                        l.LearnerId==courseInstance.LearnerId && l.CreateAt >term.BeginDate.Value.AddDays(-60)).ToList();

                var lessons = await _ablemusicContext.Lesson
                 .Where(d => d.CourseInstanceId == courseInstance.CourseInstanceId && 
                 (d.BeginTime>term.BeginDate && d.BeginTime<term.EndDate)
                ).ToArrayAsync();
                //get all orginal lessons
                foreach (var lesson in lessons)
                {
                    // if (lesson.IsCanceled ==0){
                    var isExist =  allLessons.FirstOrDefault(l => l.NewLessonId == lesson.LessonId);
                    if (isExist != null) continue;
                    var isExistMakeup = awaitMakeUpLessons.FirstOrDefault(a => a.NewLessonId==lesson.LessonId);
                    if (isExistMakeup != null) continue;
                    // }
                    var invoice = new Invoice();
                    lessonsViewModel.Add(GetLessonInfo(lesson,term.BeginDate.Value,allLessons,ref invoice));
                }
            }
            catch (Exception ex)
            {
                return lessonsViewModel;
            }
            return lessonsViewModel;
        }
        private LessonsViewModel GetLessonInfo(Lesson lesson,DateTime beginDate, List<Lesson> allLessons,ref Invoice invoice)
        {
            LessonsViewModel lessonsViewModel = new LessonsViewModel();
            lessonsViewModel.LessonId = lesson.LessonId;
            lessonsViewModel.WeekNo =(short) ((lesson.BeginTime - beginDate).Value.Days/7);
            try
            {
                //get payment info
                if (invoice == null || invoice.InvoiceNum!=lesson.InvoiceNum){
                    invoice = _ablemusicContext.Invoice
                        .FirstOrDefault(i => i.InvoiceNum == lesson.InvoiceNum
                        && i.IsActive == 1);
                }
                if ((invoice == null))
                {
                    lessonsViewModel.IsPaid = 0;
                }
                else if (invoice.OwingFee == 0)
                {
                    lessonsViewModel.IsPaid = 1;
                }
                else if (invoice.PaidFee > 0 && invoice.OwingFee > 0)
                {
                    lessonsViewModel.IsPaid = 2;
                }
                else
                {
                    lessonsViewModel.IsPaid = 0;
                }
                lessonsViewModel.OriginalDate = lesson.BeginTime.Value;
                // var actLesson = lesson;
                var newLesson = lesson;
                while (true){
                    if (newLesson.NewLessonId != null ){
                        var findLesson = allLessons.FirstOrDefault(l => l.LessonId == newLesson.NewLessonId); 
                        newLesson = findLesson;
                    }
                    else
                        break;
                     
                }
                lessonsViewModel.IsCompleted = newLesson.IsConfirm??0;
                lessonsViewModel.IsCanceled = newLesson.IsCanceled.Value;
                if (lessonsViewModel.IsCanceled ==1){
                    var awaitMakeUpLesson = _ablemusicContext.AwaitMakeUpLesson.
                    FirstOrDefault(a => a.MissedLessonId == newLesson.LessonId);
                    if (awaitMakeUpLesson== null)
                        lessonsViewModel.Remaining =0;
                    else
                        lessonsViewModel.Remaining= awaitMakeUpLesson.Remaining.Value;
                }
                if (lesson.BeginTime.Value.Date != newLesson.BeginTime.Value.Date)
                    lessonsViewModel.MakeUpDetail = newLesson.BeginTime.Value.ToString("MMM dd");
                //get makeup info
            }
            catch (Exception ex)
            {
                return lessonsViewModel;
            }
            return lessonsViewModel;
        }
    }
}