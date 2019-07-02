using System.Collections.Generic;
using System;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Pegasus_backend.Services
{
    
    /*
     * Data service 4 course remain
     */
    public class DataService
    {
        private readonly ablemusicContext _context;
        private readonly IMapper _mapper;

        public DataService(ablemusicContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<Lesson>>> GetLessons(int studentId)
        {
            Result<IEnumerable<Lesson>> result = new Result<IEnumerable<Lesson>>();
            IEnumerable<Lesson> lessons;
            try
            {
                lessons = await _context.Lesson
                    .Where(i => i.LearnerId == studentId)
                    .Include(i=>i.CourseInstance.Course)
                    .Include(i=>i.GroupCourseInstance.Course)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.IsSuccess = true;
            result.Data = lessons;
            return result;

        }

        public async Task<Result<IEnumerable<Learner>>> GetLearner(int studentId)
        {
            Result<IEnumerable<Learner>> result = new Result<IEnumerable<Learner>>();
            IEnumerable<Learner> learners;
            try
            {
                learners = await _context.Learner.Where(i => i.LearnerId == studentId)
                    .Include(i => i.One2oneCourseInstance)
                    .Include(i => i.LearnerGroupCourse).ThenInclude(i => i.GroupCourseInstance)
                    .Include(i => i.Amendment)
                    .Include(i=>i.Lesson)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.IsSuccess = true;
            result.Data = learners;
            return result;
            
            
        }

        public Result<IEnumerable<Lesson>> GetUnconfirmedLessons(int studentId)
        {
            var result = new Result<IEnumerable<Lesson>>();
            IEnumerable<Lesson> lessons;
            try
            {
                lessons = _context.Lesson.Where(i => i.LearnerId == studentId && i.IsConfirm!=1 && i.IsCanceled!=1 )
                        .Include(c=>c.Invoice);
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            
            result.IsSuccess = true;
            result.Data = lessons;
            return result;
        }
        
        //TermFilter(GetUnconfirmedLessons(studentId)) return the unconfirmed lessons with termId
        public Result<IEnumerable<LessonTerm>> TermFilter(IEnumerable<Lesson> lesson)
        {
            var lessons = lesson;
            var result = new Result<IEnumerable<LessonTerm>>();
            IEnumerable<LessonTerm> lessonWithTerm = new LessonTerm[]{};
            try
            {
                lessonWithTerm = from l in lessons
                    join i in _context.Invoice.ToList()
                        on l.InvoiceId equals i.InvoiceId
                    select new LessonTerm()
                    {
                        LessonId = l.LessonId,
                        LearnerId = l.LearnerId,
                        IsCanceled = l.IsCanceled,
                        Reason = l.Reason,
                        CourseInstanceId = l.CourseInstanceId,
                        GroupCourseInstanceId = l.GroupCourseInstanceId,
                        InvoiceId = l.InvoiceId,
                        IsConfirm = l.IsConfirm,
                        TermId = i.TermId
                    };
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.IsSuccess = true;
            result.Data = lessonWithTerm;
            return result;
           
        }
        
        public Result<IEnumerable<CourseRemain>> GetRemainLesson(int studentId)
        {
            var result = new Result<IEnumerable<CourseRemain>>();
            IEnumerable<LessonRemain> remainLessons;
            IEnumerable<CourseRemain> returnResults = new CourseRemain[]{};
            try
            {
                remainLessons = _context.LessonRemain.Where(lr => lr.LearnerId == studentId)
                    .Include(lr=>lr.Term)
                    
                .Include(lr => lr.CourseInstance)
                .ThenInclude(o2o => o2o.Course)
                .Include(lr => lr.GroupCourseInstance)
                .ThenInclude(gci => gci.Course);
                //var returnResult = _mapper.Map<IEnumerable<CourseRemain>>(remainLessons);
                foreach (var instance in remainLessons)
                {
                    var returnResult = _mapper.Map<CourseRemain>(instance);
                    returnResult.CourseName = instance.CourseInstanceId != null
                        ? instance.CourseInstance.Course
                            .CourseName
                        : instance.GroupCourseInstance.Course.CourseName;
                    returnResult.Term.LessonRemain = null;
                    returnResult.Term.Invoice = null;
                    returnResults = returnResults.Append(returnResult);
                }
                
                

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            
            result.IsSuccess = true;
            
            result.Data = returnResults;
            return result;
            
        }

        public Result<IEnumerable<CourseRemain>> CalculateQuantity(IEnumerable<Lesson> unconfirmedLessons, IEnumerable<CourseRemain> lr)
        {
            IEnumerable<CourseRemain> result = new CourseRemain[]{};
            var lessonWithTerm = TermFilter(unconfirmedLessons);
            var returnResult = new Result<IEnumerable<CourseRemain>>();
            if (!lessonWithTerm.IsSuccess)
            {
                returnResult.IsSuccess = false;
                returnResult.ErrorMessage = lessonWithTerm.ErrorMessage;
                returnResult.ErrorCode = lessonWithTerm.ErrorCode;
                return returnResult;
            }

            var unconfiremedLesson = lessonWithTerm.Data;
            
            var lessonRemains = lr;
            foreach (var lessonRemain in lessonRemains)
            {
                var unconfirm = 0;
                if (lessonRemain.CourseInstanceId != null)
                {
                    unconfirm = unconfiremedLesson
                        .Where(c => c.TermId == lessonRemain.TermId)
                        .Count(c => c.CourseInstanceId == lessonRemain.CourseInstanceId);
                }
                else
                {
                    unconfirm = unconfiremedLesson
                        .Where(c=>c.TermId == lessonRemain.TermId)
                        .Count(c => c.GroupCourseInstanceId == lessonRemain.GroupCourseInstanceId);
                }

                lessonRemain.Quantity =  lessonRemain.Quantity - unconfirm ;
                lessonRemain.Term.Invoice = null;
                lessonRemain.UnconfirmLessons = unconfirm;
                //var appendResult = _mapper.Map<CourseRemain, LessonRemainWithUnfonfirmLessons>(lessonRemain);
                //appendResult.UnconfirmLessons = unconfirm;
                
                result = result.Append(lessonRemain);
            }

            returnResult.IsSuccess = true;
            returnResult.Data = result;
            return returnResult;
        }

        /*
         *    Teacher 
         *            view
         *                 methods
         * 
         */
        
        
        public async Task<Result<IEnumerable<Lesson>>> GetLessonByTeacher(int teacherId)
        {
            Result<IEnumerable<Lesson>> result = new Result<IEnumerable<Lesson>>();
            IEnumerable<Lesson> lessons;
            try
            {
                lessons = await _context.Lesson.Where(i => i.TeacherId == teacherId).ToListAsync();
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.IsSuccess = true;
            result.Data = lessons;
            return result;
        }

        public (DateTime bgnWeek, DateTime endWeek) GetWeekInterval(DateTime date)
        {
            var week = date.DayOfWeek;
            DateTime begin;
            DateTime end;
            switch (week)
            {
                case DayOfWeek.Monday:
                    begin = date;
                    end = date.AddDays(6.0);
                    break;
                case DayOfWeek.Tuesday:
                    begin = date.AddDays(-1);
                    end = date.AddDays(5.0);
                    break;
                case DayOfWeek.Wednesday:
                    begin = date.AddDays(-2);
                    end = date.AddDays(4.0);
                    break;
                case DayOfWeek.Thursday:
                    begin = date.AddDays(-3);
                    end = date.AddDays(3.0);
                    break;
                case DayOfWeek.Friday:
                    begin = date.AddDays(-4);
                    end = date.AddDays(2.0);
                    break;
                case DayOfWeek.Saturday:
                    begin = date.AddDays(-5);
                    end = date.AddDays(1.0);
                    break;
                case DayOfWeek.Sunday:
                    begin = date.AddDays(-6);
                    end = date;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                
            }

            return (begin, end);
        }
        
        
        public Result<IEnumerable<Lesson>> FilterLessonByDate(Task<Result<IEnumerable<Lesson>>> inputLesson,DateTime inputDate)
        {
            var (begin, end) = GetWeekInterval(inputDate);
            var result = new Result<IEnumerable<Lesson>>();
            if (!inputLesson.Result.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = inputLesson.Result.ErrorMessage;
                return result;
            }

            IEnumerable<Lesson> lessons;
            try
            {
                lessons = inputLesson.Result.Data.Where(d=>DataServicePayment.Between(d.BeginTime,begin,end));
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.ErrorMessage = e.Message;
                return result;
                
            }
            result.IsSuccess = true;
            result.Data = lessons;
            return result;

        }

        public Result<double> GetHours(Result<IEnumerable<Lesson>> inputLesson)
        {
            var result = new Result<double>();
            if (!inputLesson.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = inputLesson.ErrorMessage;
                return result;
            }

            var lessons = inputLesson.Data;
            var hours = new double();
            foreach (var lesson in lessons)
            {
                if(lesson.EndTime ==null || lesson.BeginTime == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Invalid lesson's begin or end time";
                    return result;
                    
                }
                
                var hour = lesson.EndTime.GetValueOrDefault() - lesson.BeginTime.GetValueOrDefault();
                hours += hour.TotalHours ;
            }
            result.IsSuccess = true;
            result.Data = hours;
            return result;
            

        }

        public Result<Teacher> GetTeacherById(int id)
        {
            Result<Teacher> result = new Result<Teacher>();
            var teacher = new Teacher();
            try
            {
                teacher =  _context.Teacher.FirstOrDefault(i => i.TeacherId == id);
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.IsSuccess = true;
            result.Data = teacher;
            return result;
            
        }
        
        
        public Result<bool> IsMinimumHoursReached (int teacherId, Result<IEnumerable<Lesson>> inputLesson)
        {
            var result = new Result<bool>();
            var teacher = GetTeacherById(teacherId);
            var lessonsHours = GetHours(inputLesson);
            
            if (!teacher.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = teacher.ErrorMessage;
                return result;
            }
            if (teacher.Data.MinimumHours==null)
            {
                result.Note = "No Minimum Hours Requirement";
                result.IsSuccess = true;
                result.Data = true;
                return result;
            }
            
            if (!lessonsHours.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = lessonsHours.ErrorMessage;
                return result;
            }

            
            

            var hours = lessonsHours.Data;
            if (teacher.Data.MinimumHours<=hours)
            {
                result.IsSuccess = true;
                result.Data = true;
                return result;
            }

            
            result.IsSuccess = true;
            result.Data = false;
            return result;



        }

        public Result<double> GetHoursDiff(int id, DateTime date)
        {
            var result = new Result<double>();
            var teacher = GetTeacherById(id);
            var lessons = FilterLessonByDate(GetLessonByTeacher(id),date);
            if (!teacher.IsSuccess || !lessons.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = teacher.ErrorMessage + lessons.ErrorMessage;
                return result;
            }
            var hours = GetHours(lessons);
            if (!hours.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = hours.ErrorMessage;
                return result;
            }

            if (teacher.Data.MinimumHours==null)
            {
                result.IsSuccess = true;
                result.Note = "No Minimum Hours Requirement";
                result.Data = 0;
                return result;
            }
            
            var minHours = teacher.Data.MinimumHours.GetValueOrDefault();
            var diff = minHours - hours.Data;
            if (diff<0)
            {
                
                result.IsSuccess = true;
                result.Data = 0;
                return result;
                
            }
            result.IsSuccess = true;
            result.Data = diff;
            return result;



        }
        
        
        
    }
}