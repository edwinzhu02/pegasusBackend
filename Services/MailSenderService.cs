using MimeKit;
using System;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace Pegasus_backend.Services
{
    public class MailSenderService
    {

        public static Task SendMailAsync(string mailTo, string mailTitle, string mailContent, int remindLogId)
        {
            return Task.Factory.StartNew(() =>
            {
                var message = new MimeMessage();
                //message.To.Add(new MailboxAddress(mailTo));
                message.To.Add(new MailboxAddress("jy37110@gmail.com"));
                message.From.Add(new MailboxAddress("LuxeDreamEventHire", "luxecontacts94@gmail.com"));
                message.Subject = mailTitle;
                var builder = new BodyBuilder();
                builder.HtmlBody = mailContent;
                message.Body = builder.ToMessageBody();

                using (var emailClient = new SmtpClient())
                {
                    emailClient.Connect("smtp.gmail.com", 587, false);
                    emailClient.Authenticate("luxecontacts94@gmail.com", "luxe1234");
                    emailClient.Send(message);
                    emailClient.Disconnect(true);
                }

                pegasusContext.pegasusContext pegasusContext = new pegasusContext.pegasusContext();
                var remindLog = pegasusContext.RemindLog.Where(r => r.RemindId == remindLogId).FirstOrDefault();
                remindLog.EmailAt = DateTime.Now;
                remindLog.ReceivedFlag = 1;
                pegasusContext.SaveChanges();
                pegasusContext.Dispose();
            });
        }
    }
}
