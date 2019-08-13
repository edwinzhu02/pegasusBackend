using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Pegasus_backend.Utilities;
using Pegasus_backend.Services;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace Pegasus_backend.Services
{
    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private LessonGenerateService _lessonGenerateService;
        private GroupCourseGenerateService _groupCourseGenerateService;

        public TimedHostedService(ILogger<TimedHostedService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TimeSpan scheduleTime = new TimeSpan(23, 0, 0);
            TimeSpan currentTime = DateTime.UtcNow.ToNZTimezone().TimeOfDay;
            TimeSpan dueTime = scheduleTime.Subtract(currentTime);
            _timer = new Timer(DoWork, null, dueTime,
                TimeSpan.FromHours(24));
            _logger.LogInformation("Setting up the time host service. Everyday run at 23pm");
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Daily Execution Begin...");
            using(var scope = _scopeFactory.CreateScope())
            {
                var ablemusicContext = scope.ServiceProvider.GetRequiredService<ablemusicContext>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                _lessonGenerateService = new LessonGenerateService(ablemusicContext, mapper);
                await _lessonGenerateService.GetTerm(DateTime.UtcNow.ToNZTimezone(), 3);

                _groupCourseGenerateService = new GroupCourseGenerateService(ablemusicContext, _logger);
                _logger.LogInformation("Daily Execution Start at " + DateTime.UtcNow.ToNZTimezone());
                var arrangeLessonResult = new Result<string>();
                try
                {
                    arrangeLessonResult = await TermCheckingForArrangeLesson(ablemusicContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            } 
            //run auto-generate invoive and lesson
           
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        //private async Task<List<Term>> GetTerms(ablemusicContext ablemusicContext)
        //{
        //    var terms = await ablemusicContext.Term.ToListAsync();
        //    return terms;
        //}

        private async Task<Result<string>> TermCheckingForArrangeLesson(ablemusicContext ablemusicContext)
        {
            var result = new Result<string>();
            _logger.LogInformation("Checking if today is the end of any term...");
            var terms = await ablemusicContext.Term.OrderBy(t => t.BeginDate).ToListAsync();
            var today = DateTime.UtcNow.ToNZTimezone();
            Term currentTerm = null;
            Term targetTerm = null;
            int targetTermIndex = -1;
            for (int i = 0; i < terms.Count; i++)
            {
                if (today.Date >= terms[i].BeginDate.Value.Date && today.Date <= terms[i].EndDate.Value.Date)
                {
                    currentTerm = terms[i];
                    targetTermIndex = i + 2;
                }
                targetTermIndex = targetTermIndex < terms.Count ? targetTermIndex : -1;
            }
            if (currentTerm == null)
            {
                _logger.LogInformation("Today is not in any term duration");
            } 
            else
            {
                if (today.Date == currentTerm.EndDate.Value.Date)
                {
                    _logger.LogInformation("Today is the end of term " + currentTerm.TermName);
                    if (targetTermIndex < terms.Count)
                    {
                        targetTerm = terms[targetTermIndex];
                        _logger.LogInformation("Creating lessons for the term " + targetTerm.TermName + " ...");
                        result = await ArrageLessons(targetTerm.TermId);
                        _logger.LogInformation(result.Data);
                    }
                    else
                    {
                        throw new Exception("The term after next term is not found");
                    }
                }
                else
                {
                    _logger.LogInformation("Current Term is " + currentTerm.TermName + ". It is not the end of the term. No lesson to arrange for today.");
                }
            }
            return result;
        }

        private async Task<Result<string>> ArrageLessons(short termId)
        {
            var result = await _groupCourseGenerateService.GenerateLessons(termId);
            return result;
        }
    }
}
