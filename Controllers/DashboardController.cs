using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;


namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BasicController
    {
        public DashboardController(ablemusicContext ablemusicContext, ILogger<DashboardController> log) : base(ablemusicContext, log)
        {
        }

        // GET: api/Dashboard
        [HttpGet("{orgIDsStr}")]
        public async Task<IActionResult> Get(string orgIDsStr)
        {
            var result = new Result<Object>();
            var orgIDsConvertResult = orgIDsStr.ToListOfID();
            if (!orgIDsConvertResult.IsSuccess)
            {
                return BadRequest(orgIDsConvertResult);
            }

            List<int> orgIDs = orgIDsConvertResult.Data;
            var dashboardService = new DashboardService(_ablemusicContext, _log, orgIDs);

            var lessonsForToday = await dashboardService.getLessonsForToday();
            var trialLessonsForToday = await dashboardService.getTrialLessonsForToday();
            var newStudentsForToday = await dashboardService.getNewStudentsForToday();
            var waitingInvoices = await dashboardService.getUnConfirmedWaitingInvoice();
            var canceledLessonsForToday = await dashboardService.getCanceledLessonsForToday();
            var rearrangedLessonsForToday = await dashboardService.getRearrangedLessonsForToday();
            var expiredDayOffForToday = await dashboardService.getExpiredDayOffForToday();
            var expiredRearrangedLessonForToday = await dashboardService.getExpiredRearrangedLessonsForToday();
            var studentWith0RemainLessonForToday = await dashboardService.getLearnerWithRemainLessonsForToday(0);
            var studentWith1RemainLessonForToday = await dashboardService.getLearnerWithRemainLessonsForToday(1);
            var studentWith2RemainLessonsForToday = await dashboardService.getLearnerWithRemainLessonsForToday(2);
            var lessonsForRecent14Days = await dashboardService.getRecentLessons(14, toNZTimezone(DateTime.UtcNow));
            var newEnrolledStudentsForRecent8Weeks = await dashboardService.getRecentNewRegisteredLearner(8);

            var lessonsForRecent14DaysCount = new Dictionary<string, int>();
            if (lessonsForRecent14Days.IsSuccess)
            {
                foreach (var l in lessonsForRecent14Days.Data)
                {
                    lessonsForRecent14DaysCount.Add(l.Key.ToString("MM/dd/yyyy"), l.Value.Count);
                }
            } else
            {
                result.IsSuccess = false;
                result.ErrorMessage = lessonsForRecent14Days.ErrorMessage;
                return BadRequest(result);
            }
            
            var newEnrolledStudentsForRecent8WeeksCount = new Dictionary<string, int>();
            if (newEnrolledStudentsForRecent8Weeks.IsSuccess)
            {
                foreach (var ns in newEnrolledStudentsForRecent8Weeks.Data)
                {
                    newEnrolledStudentsForRecent8WeeksCount.Add(ns.Key, ns.Value.Count);
                }
            }
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = newEnrolledStudentsForRecent8Weeks.ErrorMessage;
                return BadRequest(result);
            }

            try
            {
                result.Data = new
                {
                    lessonsForToday = lessonsForToday.IsSuccess == true ? lessonsForToday.Data.Count() : throw new Exception(lessonsForToday.ErrorMessage),
                    trialLessonsForToday = trialLessonsForToday.IsSuccess == true ? trialLessonsForToday.Data.Count() : throw new Exception(trialLessonsForToday.ErrorMessage),
                    newStudentsForToday = newStudentsForToday.IsSuccess == true ? newStudentsForToday.Data.Count() : throw new Exception(newStudentsForToday.ErrorMessage),
                    waitingInvoices = waitingInvoices.IsSuccess == true ? waitingInvoices.Data.Count() : throw new Exception(waitingInvoices.ErrorMessage),
                    canceledLessonsForToday = canceledLessonsForToday.IsSuccess == true ? canceledLessonsForToday.Data.Count() : throw new Exception(canceledLessonsForToday.ErrorMessage),
                    rearrangedLessonsForToday = rearrangedLessonsForToday.IsSuccess == true ? rearrangedLessonsForToday.Data.Count() : throw new Exception(rearrangedLessonsForToday.ErrorMessage),
                    expiredDayOffForToday = expiredDayOffForToday.IsSuccess == true ? expiredDayOffForToday.Data.Count() : throw new Exception(expiredDayOffForToday.ErrorMessage),
                    expiredRearrangedLessonForToday = expiredRearrangedLessonForToday.IsSuccess == true ? expiredRearrangedLessonForToday.Data.Count() : throw new Exception(expiredRearrangedLessonForToday.ErrorMessage),
                    studentWith0RemainLessonForToday = studentWith0RemainLessonForToday.IsSuccess == true ? studentWith0RemainLessonForToday.Data.Count() : throw new Exception(studentWith0RemainLessonForToday.ErrorMessage),
                    studentWith1RemainLessonForToday = studentWith1RemainLessonForToday.IsSuccess == true ? studentWith1RemainLessonForToday.Data.Count() : throw new Exception(studentWith1RemainLessonForToday.ErrorMessage),
                    studentWith2RemainLessonsForToday = studentWith2RemainLessonsForToday.IsSuccess == true ? studentWith2RemainLessonsForToday.Data.Count() : throw new Exception(studentWith2RemainLessonsForToday.ErrorMessage),
                    lessonsForRecent14Days = lessonsForRecent14DaysCount,
                    newEnrolledStudentsForRecent8Weeks = newEnrolledStudentsForRecent8WeeksCount,
                    applyedOrgIds = orgIDs
                };
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
