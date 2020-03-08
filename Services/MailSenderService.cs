using MimeKit;
using System;
using System.IO;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;

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
        public static bool SendMail(string mailTo, string mailTitle, string mailContent)
        {
            try
            {
                var message = new MimeMessage();
                message.To.Add(new MailboxAddress(mailTo));
                message.From.Add(new MailboxAddress("AbleMusicStudio", "ablemusicstudio10@gmail.com"));
                message.Subject = mailTitle;
                
                var builder = new BodyBuilder();
                builder.HtmlBody = mailContent;
                // builder.Attachments.Add(attachment);
                message.Body = builder.ToMessageBody();
                using (var emailClient = new MailKit.Net.Smtp.SmtpClient())
                {
                    emailClient.Connect("smtp.gmail.com", 587, false);
                    emailClient.Authenticate("ablemusicstudio10@gmail.com", "er345ssDl5Ddxss");
                    emailClient.Send(message);
                    emailClient.Disconnect(true);
                }

                Console.WriteLine("Email has been sent\nMail To: " + mailTo + "\n MailTitle: " + mailTitle + "\nMail Content: " + mailContent + "\n");
                return true;
            }
            catch (Exception e)
            {
                throw e;
                // Console.WriteLine(e.Message);
                // return false;
            }
        }
        public static bool SendMailWithAttach(string mailTo, string mailTitle, string mailContent,IFormFile file)
        {
            try
            {
                var message = new MimeMessage();
                message.To.Add(new MailboxAddress(mailTo));
                message.From.Add(new MailboxAddress("AbleMusicStudio", "ablemusicstudio10@gmail.com"));
                message.Subject = mailTitle;
                
                var builder = new BodyBuilder();
                builder.HtmlBody = mailContent;

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                    builder.Attachments.Add("Invoice.pdf", fileBytes);
                }
                // builder.Attachments.Add(attachment);
                message.Body = builder.ToMessageBody();
                using (var emailClient = new MailKit.Net.Smtp.SmtpClient())
                {
                    emailClient.Connect("smtp.gmail.com", 587, false);
                    // emailClient.Authenticate("ablemusicstudio10@gmail.com", "ablemusic1234");
                    emailClient.Authenticate("ablemusicstudio10@gmail.com", "er345ssDl5Ddxss");                    
                    emailClient.Send(message);
                    emailClient.Disconnect(true);
                }

                Console.WriteLine("Email has been sent\nMail To: " + mailTo + "\n MailTitle: " + mailTitle + "\nMail Content: " + mailContent + "\n");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
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
