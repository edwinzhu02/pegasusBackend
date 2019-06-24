using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pegasus_backend.pegasusContext;


namespace Pegasus_backend.Controllers.MobileControllers
{
    public class TimeManager: BasicController
    {
        private readonly Timer _timer;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly Action _action;

        public DateTime TimerStarted { get; }

        public TimeManager(Action action, ablemusicContext ablemusicContext, ILogger<ValuesController> log) : base(ablemusicContext, log)
        {
            _action = action;
            _autoResetEvent = new AutoResetEvent(false);
            // execute every two seconds, time will make a one-second pause before first execution
            _timer = new Timer(Execute, _autoResetEvent, 1000, 2000);
            TimerStarted = toNZTimezone(DateTime.UtcNow);
        }

        public void Execute (object message)
        {
            _action();
            // 10 seconds time slot for execution
            if ((toNZTimezone(DateTime.UtcNow) - TimerStarted).Seconds > 10)
            {
                _timer.Dispose();
            }
        }
    }
}
