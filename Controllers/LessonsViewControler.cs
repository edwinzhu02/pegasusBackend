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
                    .Include(o =>o.Course)
                    .Where(d => d.OrgId == orgId
                    && (d.EndDate == null || d.EndDate > term.BeginDate)
                    ).ToArrayAsync();


                foreach (var courseInstance in courseInstances)
                {
                    var lessonView = await GetOne2OneLessons(courseInstance,term);
                    LearnersLesson.Add(
                        new LearnersLessonViewModel
                        {
                            LearnerId = courseInstance.LearnerId.Value,
                            FirstName = courseInstance.Learner.FirstName,
                            LastName = courseInstance.Learner.LastName,
                            Course = courseInstance.Course.CourseName,
                            LessonsViewModel = lessonView
                        });
                }
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
                        l.LearnerId==courseInstance.LearnerId).ToList();
                var lessons = await _ablemusicContext.Lesson
                 .Where(d => d.CourseInstanceId == courseInstance.CourseInstanceId && 
                 d.BeginTime>term.BeginDate && d.BeginTime>term.EndDate
                ).ToArrayAsync();

                foreach (var lesson in lessons)
                {
                    var isExist =  allLessons.FirstOrDefault(l => l.NewLessonId == lesson.LessonId);
                    if (isExist != null) continue;
                    lessonsViewModel.Add(GetLessonInfo(lesson,term.BeginDate.Value,allLessons));
                }
            }
            catch (Exception ex)
            {
                return lessonsViewModel;
            }
            return lessonsViewModel;
        }
        private LessonsViewModel GetLessonInfo(Lesson lesson,DateTime beginDate, List<Lesson> allLessons)
        {
            LessonsViewModel lessonsViewModel = new LessonsViewModel();

            lessonsViewModel.WeekNo =(short) ((lesson.BeginTime - beginDate).Value.Days/7);
            try
            {
                //get payment info
                var invoice = _ablemusicContext.Invoice
                    .FirstOrDefault(i => i.InvoiceNum == lesson.InvoiceNum
                    && i.IsActive == 1);
                if ((invoice == null))
                {
                    lessonsViewModel.IsPaid = 0;
                }
                else if (invoice.PaidFee > 0)
                {
                    lessonsViewModel.IsPaid = 2;
                }
                else if (invoice.PaidFee == 0)
                {
                    lessonsViewModel.IsPaid = 0;
                }
                else
                {
                    lessonsViewModel.IsPaid = 1;
                }
                if (lesson.IsConfirm==1) {
                    lessonsViewModel.IsCompleted =1;
                    lessonsViewModel.IsCanceled =0;
                    lessonsViewModel.IsMadeup =0;
                };
                if (lesson.IsCanceled==1) {
                    int? tmpLessonId = lesson.NewLessonId;

                    if (tmpLessonId ==null) {
                        lessonsViewModel.IsCanceled = 0;
                    }
                    else{
                        Lesson tmpLesson =  new Lesson();
                        while(true){
                             tmpLesson = allLessons
                                .FirstOrDefault(i => i.LessonId == tmpLessonId); 
                            if (tmpLesson.NewLessonId != null){
                                tmpLessonId = tmpLesson.NewLessonId;
                                continue;
                            }
                            break;
                        }
                        if (lesson.IsCanceled !=1 ) {
                            lessonsViewModel.IsCompleted =tmpLesson.IsConfirm.Value;
                            lessonsViewModel.IsCanceled =1;
                            lessonsViewModel.IsMadeup =1;
                            lessonsViewModel.MakeUpDetail = tmpLesson.BeginTime.Value.ToString();
                        }else{
                            var awaitMakeUpLesson = _ablemusicContext.AwaitMakeUpLesson
                                .FirstOrDefault(i => i.MissedLessonId == tmpLessonId); 
     
                            if (awaitMakeUpLesson.Remaining==0){
                                lessonsViewModel.IsCompleted = 1;
                            }else{
                                lessonsViewModel.IsCompleted = 0;
                            }
                            lessonsViewModel.IsCanceled = 1;
                            lessonsViewModel.IsMadeup = 1;
                            lessonsViewModel.MakeUpDetail = tmpLesson.BeginTime.Value.ToString();
                            }
                        }
                    }else{
                        lessonsViewModel.IsCompleted =0;
                        lessonsViewModel.IsCanceled =0;
                        lessonsViewModel.IsMadeup =0; 
                        lessonsViewModel.MakeUpDetail= lesson.BeginTime.Value.ToString();                
                    }
                
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