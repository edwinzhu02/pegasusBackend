using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Controllers
{
    [Route("invoice")]
    [ApiController]
    public class InvoiceGenerateController : ControllerBase
    {
        private readonly pegasusContext.ablemusicContext _context;
        private List<Learner> StudentInfo;
        private decimal? price;
        private IEnumerable<Holiday> holidays;
        
        public InvoiceGenerateController(pegasusContext.ablemusicContext context)
        {
            _context = context;
            holidays = _context.Holiday.ToList();
        }
        
        //GET 
        
        /*
         * Get the student information
         *    From the database
         */
        [HttpGet("l/{studentId}")]
        public ActionResult<List<Learner>> GetInfo(int studentId)
        {
            Result<List<Learner>> result = new Result<List<Learner>>();
            try
            {
                StudentInfo = _context.Learner.Where(s => s.LearnerId == studentId)
                    .Include(s => s.One2oneCourseInstance)
                    .ThenInclude(s=>s.CourseSchedule)
                    .Include(s => s.One2oneCourseInstance)
                    .ThenInclude(s=>s.Course)
                    
                    .Include(s => s.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.Course)
                    
                    .Include(s => s.LearnerGroupCourse)
                    .ThenInclude(s => s.GroupCourseInstance)
                    .ThenInclude(s => s.CourseSchedule)
                    
                    .ToList();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }

            return StudentInfo;
        }

        
        /*
         *  Get one 2 one courses, schedule and amendment 
         *     return type
         *     (IEnumerable<courses>,IEnumerable<courseSchedules>,IEnumerable<Amendment>)
         */
        [HttpGet("getc/{id}")]
        public IEnumerable<CourseInvoice> GetCourses(int id)
        {
            IEnumerable<CourseInvoice> courseInvoices = new CourseInvoice[]{};
            var student = GetInfo(id).Value.ElementAt(0);
            //one 2 one courses
            var one2OneInstance = student.One2oneCourseInstance;
            if (one2OneInstance.Any())
            {
                foreach (var instance in one2OneInstance)
                {
                    
                    CourseInvoice courseInvoice = new CourseInvoice();
                    courseInvoice.One2OneCourseInstanceId = instance.CourseInstanceId;
                    
                    courseInvoice.CourseSchedules =instance.CourseSchedule;
                    
                    courseInvoice.CourseAmendments =instance.Amendment;

                    courseInvoice.CoursePrice =instance.Course.Price;

                    courseInvoice.Course = instance.Course;

                    courseInvoice.LeanerId = instance.LearnerId;

                    courseInvoice.TeacherId = instance.TeacherId;

                    courseInvoice.One2oneCourseInstance = instance;

                    courseInvoice.begin_date = instance.BeginDate;

                    courseInvoice.end_date = instance.EndDate;
                    
                    courseInvoices = courseInvoices.Append(courseInvoice);


                }
            }
            
            //group instance
            IEnumerable<GroupCourseInstance> groupCoursesCollection = new GroupCourseInstance[]{}; 
            var leanerGroupInstance = student.LearnerGroupCourse;
            if (leanerGroupInstance.Any())
            {
                foreach (var lgc in leanerGroupInstance)//get GroupCourseInstance from learnerGroupCourse
                {
                    groupCoursesCollection.Append(lgc.GroupCourseInstance);
                }

                foreach (var instance in groupCoursesCollection)
                {
                    CourseInvoice courseInvoice = new CourseInvoice();

                    courseInvoice.IsGroupCourse = true;
                    courseInvoice.GroupCourseInstance = instance;
                    courseInvoice.GroupCourseInstanceId = instance.GroupCourseInstanceId;
                    courseInvoice.Course = instance.Course;
                    courseInvoice.CoursePrice = courseInvoice.Course.Price;
                    courseInvoice.TeacherId = instance.TeacherId;
                    courseInvoice.CourseSchedules = instance.CourseSchedule;
                    courseInvoice.LeanerId = student.LearnerId;
                    courseInvoice.begin_date = instance.BeginDate;
                    courseInvoice.end_date = instance.EndDate;
                    
                    courseInvoices = courseInvoices.Append(courseInvoice);

                }
                
                
                
                
            }



            return courseInvoices;
        }

        public static bool Between(DateTime? input, DateTime? date1, DateTime? date2)
        {
            return (input > date1 && input < date2);
        }


        
        //public int DeductHoliday(IEnumerable<CourseInvoice> instances)
        //{
        //    int numberOfHolidays=0;
        //    foreach (var instance in instances)
        //    {
        //        if (instance.IsGroupCourse)
        //        {
        //            Console.Write("Group course");
        //        }

        //        foreach (var holiday in holidays)
        //        {
        //            if (Between(holiday.HolidayDate,instance.One2oneCourseInstance.BeginDate,instance.One2oneCourseInstance.EndDate))
        //            {
        //                numberOfHolidays += 1;
        //            }
        //        }
        //    }

        //    return numberOfHolidays;
        //}

        
        
        
        //[HttpGet("gh/{id}")]
        //public int getHoliday(int id)
        //{
           
        //   return DeductHoliday(GetCourses(id));


        //}
       
        /*
        [HttpGet("ts/{studentId}")]
        
        public ActionResult<List<GroupCourseInstance>> Get(int studentId)
        {
            var groupCourse = _context.LearnerGroupCourse
                .Where(s => s.LearnerId == studentId)
                .Join(_context.GroupCourseInstance,
                    lg => lg.GroupCourseInstanceId,
                    g => g.GroupCourseInstanceId,
                    (lg, g) => g).ToList();
            return groupCourse;
        }
        */



        //POST
        [HttpPost]
        public /* - ActionResult<Invoice> -*/ void GenerateInvoice ()
        {
            

        }
        
        
    }
}