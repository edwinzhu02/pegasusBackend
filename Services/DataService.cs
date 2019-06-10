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
        



    }
}