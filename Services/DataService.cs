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

namespace Pegasus_backend.Services
{
    
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
            IEnumerable<Lesson> lessons = new Lesson[]{};
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
            IEnumerable<Learner> learners = new Learner[]{};
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

        public async Task<Result<IEnumerable<Lesson>>> GetUnconfirmedLessons(int studentId)
        {
            Result<IEnumerable<Lesson>> result = new Result<IEnumerable<Lesson>>();
            IEnumerable<Lesson> lessons = new Lesson[]{};
            try
            {
                lessons = await _context.Lesson.Where(i => i.LearnerId == studentId).Where(c=>c.IsConfirm!=1).ToListAsync();
                
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
        
        public async Task<Result<IEnumerable<LessonRemain>>> GetRemainLesson(int studentId)
        {
            Result<IEnumerable<LessonRemain>> result = new Result<IEnumerable<LessonRemain>>();
            IEnumerable<LessonRemain> remainLessons = new LessonRemain[]{};
            try
            {
                remainLessons = await _context.LessonRemain.Where(i => i.LearnerId == studentId)
                    .Include(i=>i.GroupCourseInstance)
                    .Include(i=>i.CourseInstance)
                    .Include(i=>i.Term)
                    .ToListAsync();
                
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
        
        



    }
}