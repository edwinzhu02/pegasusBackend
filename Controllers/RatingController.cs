using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Utilities;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController: BasicController
    {
        public RatingController(ablemusicContext ablemusicContext, ILogger<LessonRescheduleController> log) : base(ablemusicContext, log){}

        [HttpGet("[action]/{userId}/{index}")]
        public async Task<IActionResult> TeacherGetRating(short userId,int index)
        {
            var result = new Result<object>();
            try
            {
                var teacher = _ablemusicContext.Teacher.FirstOrDefault(s => s.UserId == userId);
                if (teacher == null)
                {
                    throw new Exception("You are not teacher.");
                }

                var teacherId = teacher.TeacherId;
                //0 is student to teacher 1 is teacher to student 2 is teacher to school
                var ratingItem = await _ablemusicContext.Rating
                    .Include(s=>s.Learner)
                    .Include(s=>s.Lesson)
                    .Where(s => s.TeacherId == teacherId && s.RateType == 0)
                    .Select(s=>new {s.Learner.FirstName,s.Learner.LastName,s.Lesson.BeginTime,s.Comment,s.RateStar,s.CreateAt})
                    .OrderByDescending(s=>s.CreateAt)
                    .Skip(index*10)
                    .Take(10)
                    .ToListAsync();
                result.Data = ratingItem;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        

        [HttpGet("[action]/{userId}/{index}")]
        public async Task<IActionResult> LearnerGetRating(short userId,int index)
        {
            var result = new Result<object>();
            try
            {
                var learner = _ablemusicContext.Learner.FirstOrDefault(s => s.UserId == userId);
                if (learner == null)
                {
                    throw new Exception("You are not Learner");
                }

                var learnerId = learner.LearnerId;
                var ratingItem = await _ablemusicContext.Rating
                    .Include(s=>s.Teacher)
                    .Include(s=>s.Lesson)
                    .Where(s => s.LearnerId == learnerId && s.RateType == 1)
                    .Select(s=>new {s.Teacher.FirstName,s.Teacher.LastName,s.Lesson.BeginTime,s.Comment,s.RateStar,s.CreateAt})
                    .OrderByDescending(s=>s.CreateAt)
                    .Skip(index*10)
                    .Take(10)
                    .ToListAsync();
                result.Data = ratingItem;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        

        [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> TeacherFeedbackRatingList(short userId)
        {
            var result = new Result<object>();
            try
            {
                var teacher = _ablemusicContext.Teacher.FirstOrDefault(s => s.UserId == userId);
                if (teacher == null)
                {
                    throw new Exception("You are not Teacher");
                }

                var teacherId = teacher.TeacherId;
                var item = await _ablemusicContext.Lesson
                    .Include(s => s.Rating)
                    .Include(s => s.GroupCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.LearnerGroupCourse)
                    .Include(s => s.CourseInstance)
                    .ThenInclude(w => w.Course)
                    .Where(s => s.IsConfirm == 1 && s.CreatedAt >= DateTime.UtcNow.AddHours(12).AddDays(-14) && s.TeacherId ==teacherId)
                    .Select(s => new
                    {
                        s.LearnerId, isRate = s.Rating.ToList().Exists(q=>q.RateType == 1)?1:0, s.LessonId,s.BeginTime,
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName
                    })
                    .ToListAsync();
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

        [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> LearnerFeedbackRatingList(short userId)
        {
            var result = new Result<object>();
            try
            {
                var learner = _ablemusicContext.Learner.FirstOrDefault(s => s.UserId == userId);
                if (learner == null)
                {
                    throw new Exception("You are not the Learner");
                }
                var learnerId = learner.LearnerId;
                var item = await _ablemusicContext.Lesson
                    .Include(s => s.Rating)
                    .Include(s => s.GroupCourseInstance)
                    .ThenInclude(w => w.Course)
                    .Include(s=>s.GroupCourseInstance)
                    .ThenInclude(s=>s.LearnerGroupCourse)
                    .Include(s => s.CourseInstance)
                    .ThenInclude(w => w.Course)
                    .Where(s => (s.LearnerId == learnerId || s.GroupCourseInstance.LearnerGroupCourse.ToList()
                                     .Exists(e => e.LearnerId == learnerId))
                                && s.IsConfirm == 1 && s.CreatedAt >= DateTime.UtcNow.AddHours(12).AddDays(-14))
                    .Select(s => new
                    {
                        s.TeacherId, isRate = s.Rating.ToList().Exists(q=>q.RateType == 0)?1:0,s.LessonId,s.BeginTime,
                        CourseName=!IsNull(s.GroupCourseInstance)?s.GroupCourseInstance.Course.CourseName:IsNull(s.CourseInstance)?s.TrialCourse.CourseName:s.CourseInstance.Course.CourseName
                    }).ToListAsync();

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

        [HttpPost("[action]")]
        public async Task<IActionResult> LearnerFeedback([FromBody] LearnerFeedbackModel model)
        {
            var result = new Result<string>();
            try
            {
                var learner = _ablemusicContext.Learner.FirstOrDefault(s => s.UserId == model.UserId);
                if (learner == null)
                {
                    throw new Exception("You are not the learner.");
                }

                var learnerId = learner.LearnerId;
                var teacherId = _ablemusicContext.Lesson.FirstOrDefault(s => s.LessonId == model.LessonId).TeacherId;
                
                var LearnerToTeacherRating = new Rating
                {
                    RateType = 0,
                    Comment = model.CommentToTeacher,
                    CreateAt = DateTime.UtcNow.AddHours(12),
                    LearnerId = learnerId,
                    TeacherId = teacherId,
                    LessonId = model.LessonId,
                    RateStar = model.RateStar
                };

                _ablemusicContext.Add(LearnerToTeacherRating);
                await _ablemusicContext.SaveChangesAsync();
                result.Data = "Comment successfully!";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TeacherFeedback([FromBody] TeacherFeedbackModel model)
        {
            var result = new Result<string>();
            try
            {
                var teacher = _ablemusicContext.Teacher.FirstOrDefault(s => model.UserId == s.UserId);
                if (teacher == null)
                {
                    throw new Exception("You are not the teacher");
                }
                var teacherId = teacher.TeacherId;
                using (var dbTransaction = _ablemusicContext.Database.BeginTransaction())
                {
                    //save the teacher to school comment firstly
                    var TeacherToSchoolRating = new Rating
                    {
                        RateType = 2,
                        Comment = model.CommentToSchool,
                        CreateAt = DateTime.UtcNow.AddHours(12),
                        TeacherId = teacherId,
                        LessonId = model.LessonId,
                        RateStar = model.RateStar,
                    };
                    _ablemusicContext.Add(TeacherToSchoolRating);
                    await _ablemusicContext.SaveChangesAsync();
                    
                    //then process the teacher to student comment
                    var lesson = _ablemusicContext.Lesson
                        .FirstOrDefault(s => s.LessonId == model.LessonId);
                    
                    var learnerId = lesson.LearnerId;
                    if (learnerId != null)
                    {
                        var TeacherToLearner = new Rating
                        {
                            RateType = 1,
                            Comment = model.CommentToLearner,
                            CreateAt = DateTime.UtcNow.AddHours(12),
                            LearnerId =  learnerId,
                            TeacherId = teacherId,
                            LessonId = model.LessonId,
                            RateStar = model.RateStar
                        };
                        _ablemusicContext.Add(TeacherToLearner);
                        await _ablemusicContext.SaveChangesAsync();
                    }

                    if (learnerId == null && lesson.GroupCourseInstanceId != null)
                    {
                        var learnerGroupCourse = _ablemusicContext.LearnerGroupCourse
                            .Where(s => s.GroupCourseInstanceId == lesson.GroupCourseInstanceId).ToList();
                        learnerGroupCourse.ForEach(s =>
                        {
                            var teacherTostudent = new Rating
                            {
                                RateType = 1,
                                Comment = model.CommentToLearner,
                                CreateAt = DateTime.UtcNow.AddHours(12),
                                LearnerId = s.LearnerId,
                                TeacherId = teacherId,
                                LessonId = model.LessonId,
                                RateStar = model.RateStar
                            };
                            _ablemusicContext.Add(teacherTostudent);
                        });
                        await _ablemusicContext.SaveChangesAsync();
                    }
                    
                    dbTransaction.Commit();
                    result.Data = "Comment successfully!";
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpGet("[action]/{lessonId}/{userId}")]
        public async Task<IActionResult> LearnerGetOneRatingHistoryById(int lessonId, short userId)
        {
            var result = new Result<object>();
            try
            {
                var learner = _ablemusicContext.Learner.FirstOrDefault(s => s.UserId == userId);
                if (learner == null)
                {
                    throw new Exception("You are not the learner.");
                }

                var learnerId = learner.LearnerId;

                var ToTeacher = await _ablemusicContext.Rating
                    .FirstOrDefaultAsync(s => s.RateType == 0 && s.LearnerId == learnerId && s.LessonId == lessonId);
                result.Data = ToTeacher;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpGet("[action]/{lessonId}/{userId}")]
        public async Task<IActionResult> TeacherGetOneRatingHistoryById(int lessonId, short userId)
        {
            var result = new Result<object>();
            try
            {
                var teacher = _ablemusicContext.Teacher.FirstOrDefault(s => s.UserId == userId);
                if (teacher == null)
                {
                    throw new Exception("You are not the teacher.");
                }

                var teacherId = teacher.TeacherId;

                var ToLearner = await _ablemusicContext.Rating
                    .FirstOrDefaultAsync(s => s.RateType == 1 &&
                        s.TeacherId == teacherId && s.LessonId == lessonId);
                
                var ToSchool = await _ablemusicContext.Rating
                    .FirstOrDefaultAsync(s => s.RateType == 2 &&
                                              s.TeacherId == teacherId && s.LessonId == lessonId);

                result.Data = new
                {
                    ToLearner, ToSchool

                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

    }
}