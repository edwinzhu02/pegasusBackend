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
                try
                {
                    //var message = new MimeMessage();
                    //message.To.Add(new MailboxAddress(mailTo));
                    //message.From.Add(new MailboxAddress("LuxeDreamEventHire", "luxecontacts94@gmail.com"));
                    //message.Subject = mailTitle;
                    //var builder = new BodyBuilder();
                    //builder.HtmlBody = mailContent;
                    //message.Body = builder.ToMessageBody();
                    //using (var emailClient = new SmtpClient())
                    //{
                    //    emailClient.Connect("smtp.gmail.com", 587, false);
                    //    emailClient.Authenticate("luxecontacts94@gmail.com", "luxe1234");
                    //    emailClient.Send(message);
                    //    emailClient.Disconnect(true);
                    //}

                    //pegasusContext.ablemusicContext ablemusicContext = new pegasusContext.ablemusicContext();
                    //var remindLog = ablemusicContext.RemindLog.Where(r => r.RemindId == remindLogId).FirstOrDefault();
                    //remindLog.EmailAt = toNZTimezone(DateTime.UtcNow);
                    //remindLog.ReceivedFlag = 1;
                    //ablemusicContext.SaveChanges();
                    //ablemusicContext.Dispose();
                    Console.WriteLine("Email has been sent\nMail To: " + mailTo + "\n MailTitle: " + mailTitle + "\nMail Content: " + mailContent + "\nRemind ID: " + remindLogId.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }

        public static Task SendMailUpdateInvoiceWaitingTableAsync(string mailTo, string mailTitle, string mailContent, int waitingId)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    //var message = new MimeMessage();
                    //message.To.Add(new MailboxAddress(mailTo));
                    //message.From.Add(new MailboxAddress("LuxeDreamEventHire", "luxecontacts94@gmail.com"));
                    //message.Subject = mailTitle;
                    //var builder = new BodyBuilder();
                    //builder.HtmlBody = mailContent;
                    //message.Body = builder.ToMessageBody();
                    //using (var emailClient = new SmtpClient())
                    //{
                    //    emailClient.Connect("smtp.gmail.com", 587, false);
                    //    emailClient.Authenticate("luxecontacts94@gmail.com", "luxe1234");
                    //    emailClient.Send(message);
                    //    emailClient.Disconnect(true);
                    //}

                    //pegasusContext.ablemusicContext ablemusicContext = new pegasusContext.ablemusicContext();
                    //var invoiceWaitingConfirm = ablemusicContext.InvoiceWaitingConfirm.Where(r => r.WaitingId == waitingId).FirstOrDefault();
                    //invoiceWaitingConfirm.IsEmailSent = 1;
                    //ablemusicContext.SaveChanges();
                    //ablemusicContext.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }
    }
}
