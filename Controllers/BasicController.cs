using Pegasus_backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
//using System.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Pegasus_backend.Services;

namespace Pegasus_backend.Controllers
{
    public abstract class BasicController: ControllerBase
    {
        protected readonly ablemusicContext _ablemusicContext;
        protected readonly ILogger<Object> _log;
        protected readonly NotificationObservable _notificationObservable;

        public BasicController(ablemusicContext ablemusicContext, ILogger<Object> log)
        {
            _ablemusicContext = ablemusicContext;
            _log = log;
            _notificationObservable = new NotificationObservable();
            _notificationObservable.SendNotification += SendNotification;
        }

        private void SendNotification(object sender, NotificationEventArgs e)
        {
            Task mailTask = MailSenderService.SendMailAsync(e.mailTo, e.mailTitle, e.mailContent, e.remindLogId);
        }

        //InovoiceTitle: PDF 标题， invoiceName：给谁发invoice，invoicemount：总共的amount， infos：看models文件夹，这个list里的东西全放在pdf里的table里、
        //filename 文件保存的名字
        protected static void InvoiceGenerator(string invoiceTitle, string invoiceName, decimal invoiceAmount, List<InvoicePdfGeneratorModel> infos,string filename)
        {
            //title
            
            PdfPTable title = new PdfPTable(1);
            title.WidthPercentage = 80;
            title.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            title.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
            title.DefaultCell.BorderWidth = 0;
            Chunk titleChunk = new Chunk(invoiceTitle, FontFactory.GetFont("Times New Roman"));
            titleChunk.Font.Color = new iTextSharp.text.BaseColor(0,0,0);
            titleChunk.Font.SetStyle(0);
            titleChunk.Font.Size = 18;
            Phrase titlePhrase = new Phrase();
            titlePhrase.Add(titleChunk);
            title.AddCell(titlePhrase);
            
            //blank
            PdfPTable pdfTableBlank = new PdfPTable(1);
            pdfTableBlank.DefaultCell.BorderWidth = 0;
            pdfTableBlank.DefaultCell.Border = 0;
            pdfTableBlank.AddCell(new Phrase(" "));
            
            //invoice to (who)
            PdfPTable name = new PdfPTable(1);
            name.WidthPercentage = 80;
            name.DefaultCell.BorderWidth = 0;
            Chunk nameChunk = new Chunk("Invoice To: "+invoiceName  , FontFactory.GetFont("Times New Roman"));
            nameChunk.Font.Color = new iTextSharp.text.BaseColor(0,0,0);
            nameChunk.Font.SetStyle(0);
            nameChunk.Font.Size = 10;
            Phrase namePhrase = new Phrase();
            namePhrase.Add(nameChunk);
            name.AddCell(namePhrase);
            
            //detail table
            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.Padding = 5;
            table.WidthPercentage = 80;
            table.DefaultCell.BorderWidth = 0.5f;
            table.AddCell(new Phrase("Title"));
            table.AddCell(new Phrase("Amount"));
            infos.ForEach(s =>
            {
                table.AddCell(s.title);
                table.AddCell("$ " + s.amount);
            });
            
            //Total Amount
            PdfPTable amount = new PdfPTable(1);
            amount.WidthPercentage = 80;
            amount.DefaultCell.BorderWidth = 0;
            Chunk amountchunk = new Chunk("Total Amount: $ "+invoiceAmount  , FontFactory.GetFont("Times New Roman"));
            amountchunk.Font.Color = new iTextSharp.text.BaseColor(0,0,0);
            amountchunk.Font.SetStyle(0);
            amountchunk.Font.Size = 14;
            Phrase amountPhrase = new Phrase();
            amountPhrase.Add(amountchunk);
            amount.AddCell(amountPhrase);


            var filenameToKeep = filename + ".pdf";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images","invoice",filenameToKeep);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4,10f,10f,10f,0f);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                pdfDoc.Add(title);
                pdfDoc.Add(pdfTableBlank);
                pdfDoc.Add(name);
                pdfDoc.Add(pdfTableBlank);
                pdfDoc.Add(table);
                pdfDoc.Add(pdfTableBlank);
                pdfDoc.Add(amount);
                pdfDoc.Close();
                stream.Close();
            }
        }
        
        
        protected TimeSpan GetEndTimeForOnetoOneCourseSchedule(TimeSpan beginTime, short? durationType)
        {
            switch (durationType)
            {
                case 1:
                    return beginTime.Add(new TimeSpan(0,30,0));
                case 2:
                    return beginTime.Add(new TimeSpan(0,45,0));
                case 3:
                    return beginTime.Add(new TimeSpan(1,0,0));
                case 4:
                    return beginTime.Add(new TimeSpan(1,15,0));
                default:
                    throw new Exception("Duration type must be From 1 to 4");
            }
        }
        
        protected void DeleteFile(string filePath)
        {
            var folderName = Path.Combine("wwwroot", filePath);
            if (System.IO.File.Exists(folderName))
            {
                System.IO.File.Delete(folderName);
            }
            else
            {
                throw new Exception("Delete Fail");
            }
        }

        protected bool IsNull<T>(T item)
        {
            return item == null;
        }

        protected Object UserInfoFilter(User user, string positionToClient)
        {
            if (user.Teacher.Count != 0)
            {
               return new {firstname=user.Teacher.ToList()[0].FirstName,lastname=user.Teacher.ToList()[0].LastName,
                   position=positionToClient};
            }

            if (user.Staff.Count != 0)
            {
                return new
                {
                    firstname = user.Staff.ToList()[0].FirstName, lastname = user.Staff.ToList()[0].LastName,
                    position = positionToClient,
                    OrgName = user.Staff.ToList()[0].StaffOrg.ToList().Select(s => s.Org.OrgName),
                    OrgId = user.Staff.ToList()[0].StaffOrg.ToList().Select(s => s.Org.OrgId),
                    StaffId =  user.Staff.FirstOrDefault().StaffId
                };
            }

            if (user.Learner.Count != 0)
            {
                return new {firstname=user.Learner.ToList()[0].FirstName,lastname=user.Learner.ToList()[0].LastName,position=positionToClient};
            }

            throw new Exception("User Not Found.");
        }
        
        protected void UpdateTable(object model, Type type, object tableRow)
        {
            var properties = model.GetType().GetProperties();
            foreach (var prop in properties)
            {
                PropertyInfo piInstance = type.GetProperty(prop.Name);
                if (piInstance != null && prop.GetValue(model) != null)
                {
                    piInstance.SetValue(tableRow, prop.GetValue(model));
                }
            }
        }

        protected Result<T> DataNotFound<T>(Result<T> tResult)
        {
            tResult.IsSuccess = false;
            tResult.IsFound = false;
            tResult.ErrorMessage = "Not Found";
            return tResult;
        }
        protected UploadFileModel UploadFile(IFormFile file,string folderName, int id,string strDateTime)
        {
            var model = new UploadFileModel();
            try
            {
                string currentFileExtension = Path.GetExtension(file.FileName);
                var saveName = id.ToString() + strDateTime + currentFileExtension;
                var newPath = Path.Combine("images", folderName, saveName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", newPath);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                
                model.IsUploadSuccess = true;
                model.ErrorMessage = "";
                return model;
            }
            catch (Exception ex)
            {
                model.IsUploadSuccess = false;
                model.ErrorMessage = ex.Message;
                return model;
            }
        }
        protected DateTime toNZTimezone(DateTime utc)
        {
            DateTime nzTime = new DateTime();
            try
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, nztZone);
                return nzTime;
            }
            catch (TimeZoneNotFoundException)
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, nztZone);
                return nzTime;
            }
            catch (InvalidTimeZoneException)
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, nztZone);
                return nzTime;
            }
        }
        
        protected void LogInfoToFile(string message)
        {
            _log.LogInformation(this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + message);
        }

        protected void LogWarningToFile(string message)
        {
            _log.LogWarning(this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + message);
        }

        protected void LogErrorToFile(string message)
        {
            _log.LogError(this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + message);
        }
    }
}