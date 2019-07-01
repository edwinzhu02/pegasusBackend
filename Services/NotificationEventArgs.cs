using System;

namespace Pegasus_backend.Services
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(string mailTo, string mailTitle, string mailContent, int remindLogId)
        {
            this.mailTo = mailTo;
            this.mailTitle = mailTitle;
            this.mailContent = mailContent;
            this.remindLogId = remindLogId;
        }
        public string mailTo { get; set; }
        public string mailTitle { get; set; }
        public string mailContent { get; set; }
        public int remindLogId { get; set; }
    }
}
