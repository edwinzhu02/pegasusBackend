using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Pegasus_backend.pegasusContext;
using System.Collections.Generic;
using Pegasus_backend.Utilities;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace Pegasus_backend.Services
{
    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(DoWorkAsync, null, TimeSpan.Zero,
                TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void DoWorkAsync(object state)
        {
            var ableMusicContext = new ablemusicContext();
            var remindLogs = new List<RemindLog>();
            try
            {
                remindLogs = await ableMusicContext.RemindLog.Where(r => r.ScheduledDate.HasValue &&
                r.ScheduledDate.Value.Date == DateTime.UtcNow.ToNZTimezone().Date).ToListAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError("Fail to read remindlog from db");
                _logger.LogError(ex.Message);
            }
            if(remindLogs.Count <= 0)
            {
                _logger.LogInformation("No Email to send for today");
            }
            else
            {
                _logger.LogInformation(remindLogs.Count + "Email has been sent for today");
            }
            //sending Email
            _logger.LogInformation("DoWork executed");
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
