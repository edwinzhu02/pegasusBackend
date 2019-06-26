﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Pegasus_backend.Services;

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
            string[] orgIDsArr;
            List<int> orgIDs = new List<int>();
            if (orgIDsStr.Length <= 0)
            {
                orgIDsArr = new string[] { };
                result.IsSuccess = false;
                result.ErrorMessage = "orgID is required";
                return BadRequest(result);
            } else if(orgIDsStr.Length == 1)
            {
                orgIDsArr = new string[] { orgIDsStr };
            } else
            {
                orgIDsArr = orgIDsStr.Split(new char[] { ',' });
            }
            for (var i = 0; i < orgIDsArr.Length; i++)
            {
                try
                {
                    orgIDs.Add(Int32.Parse(orgIDsArr[i]));
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return BadRequest(result);
                }
            }

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
            foreach(var l in lessonsForRecent14Days)
            {
                lessonsForRecent14DaysCount.Add(l.Key.ToString("MM/dd/yyyy"), l.Value.Count);
            }
            var newEnrolledStudentsForRecent8WeeksCount = new Dictionary<string, int>();
            foreach(var ns in newEnrolledStudentsForRecent8Weeks)
            {
                newEnrolledStudentsForRecent8WeeksCount.Add(ns.Key, ns.Value.Count);
            }


            result.Data = new
            {
                lessonsForToday = lessonsForToday.Count(),
                trialLessonsForToday = trialLessonsForToday.Count(),
                newStudentsForToday = newStudentsForToday.Count(),
                waitingInvoices = waitingInvoices.Count(),
                canceledLessonsForToday = canceledLessonsForToday.Count(),
                rearrangedLessonsForToday = rearrangedLessonsForToday.Count(),
                expiredDayOffForToday = expiredDayOffForToday.Count(),
                expiredRearrangedLessonForToday = expiredRearrangedLessonForToday.Count(),
                studentWith0RemainLessonForToday = studentWith0RemainLessonForToday.Count(),
                studentWith1RemainLessonForToday = studentWith1RemainLessonForToday.Count(),
                studentWith2RemainLessonsForToday = studentWith2RemainLessonsForToday.Count(),

                lessonsForRecent7Days = lessonsForRecent14DaysCount,
                newEnrolledStudentsForRecent8Weeks = newEnrolledStudentsForRecent8WeeksCount,
                applyedOrgIds = orgIDs
            };

            return Ok(result);
        }
    }
}
