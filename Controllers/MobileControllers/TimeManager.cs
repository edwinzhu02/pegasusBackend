using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pegasus_backend.Controllers.MobileControllers
{
    public class TimeManager
    {
        private readonly Timer _timer;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly Action _action;

        public DateTime TimerStarted { get; }

        public TimeManager(Action action)
        {
            _action = action;
            _autoResetEvent = new AutoResetEvent(false);
            // execute every two seconds, time will make a one-second pause before first execution
            _timer = new Timer(Execute, _autoResetEvent, 1000, 2000);
            TimerStarted = DateTime.Now;
        }

        public void Execute (object message)
        {
            _action();
            // 10 seconds time slot for execution
            if ((DateTime.Now - TimerStarted).Seconds > 10)
            {
                _timer.Dispose();
            }
        }
    }
}
