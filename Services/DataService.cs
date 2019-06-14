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

        public DataService(ablemusicContext context)
        {
            _context = context;
        }

        public async Task<Result<IEnumerable<Lesson>>> GetLessons(int studentId)
        {
            Result<IEnumerable<Lesson>> result = new Result<IEnumerable<Lesson>>();
            IEnumerable<Lesson> lessons;
            try
            {
                lessons = await _context.Lesson.Where(i => i.LearnerId == studentId).ToListAsync();
                
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
                lessons = _context.Lesson.Where(i => i.LearnerId == studentId)
                        .Where(c=>c.IsConfirm!=1)
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
        
        public Result<IEnumerable<LessonRemain>> GetRemainLesson(int studentId)
        {
            var result = new Result<IEnumerable<LessonRemain>>();
            IEnumerable<LessonRemain> remainLessons;
            try
            {
                remainLessons = _context.LessonRemain.Where(i => i.LearnerId == studentId);
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            
            result.IsSuccess = true;
            result.Data = remainLessons;
            return result;
            
        }

        public Result<IEnumerable<LessonRemain>> CalculateQuantity(IEnumerable<Lesson> unconfirmedLessons, IEnumerable<LessonRemain> lr)
        {
            IEnumerable<LessonRemain> result = new LessonRemain[]{};
            var lessonWithTerm = TermFilter(unconfirmedLessons);
            var returnResult = new Result<IEnumerable<LessonRemain>>();
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
                if (lessonRemain.CourseInstance != null)
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
                result = result.Append(lessonRemain);
            }

            returnResult.IsSuccess = true;
            returnResult.Data = result;
            return returnResult;
        }

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
        
        
    }
}