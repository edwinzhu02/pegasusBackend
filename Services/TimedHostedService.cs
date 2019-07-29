using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Services
{
    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly LessonGenerateService _lessonGenerateService;

        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            _logger = logger;
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            Console.WriteLine("DoWork executed");

            //run auto-generate invoive and lesson
            //await _lessonGenerateService.GetTerm(DateTime.UtcNow.ToNZTimezone(),3);

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
    }
}
