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

namespace Pegasus_backend.Controllers
{
    public abstract class BasicController: ControllerBase
    {
        protected readonly ablemusicContext _ablemusicContext;
        protected readonly ILogger<ValuesController> _log;

        public BasicController(ablemusicContext ablemusicContext, ILogger<ValuesController> log)
        {
            _ablemusicContext = ablemusicContext;
            _log = log;
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
                    OrgId = user.Staff.ToList()[0].StaffOrg.ToList().Select(s => s.Org.OrgId)
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
            DateTime nzTime = utc;
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
                return nzTime;
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("The registry does not define the Central Standard Time zone.");
                return nzTime;
            }
            catch (InvalidTimeZoneException)
            {
                Console.WriteLine("Registry data on the Central Standard Time zone has been corrupted.");
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