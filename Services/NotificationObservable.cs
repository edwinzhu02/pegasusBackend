using System;

namespace Pegasus_backend.Services
{
    public class NotificationObservable
    {
        public event EventHandler<NotificationEventArgs> SendNotification;

        public void send(NotificationEventArgs e)
        {
            OnSendNotification(e);
        }

        protected virtual void OnSendNotification(NotificationEventArgs e)
        {
            EventHandler<NotificationEventArgs> handler = SendNotification;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
