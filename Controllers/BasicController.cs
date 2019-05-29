using Pegasus_backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
//using System.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    public abstract class BasicController: ControllerBase
    {
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
                return new {firstname=user.Staff.ToList()[0].FirstName,lastname=user.Staff.ToList()[0].LastName,
                    position=positionToClient, OrgName=user.Staff.ToList()[0].StaffOrg.ToList().Select(s=>s.Org.OrgName)};
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
        protected void UploadFile(IFormFile file,string imageName, int id)
        {
            var fileName = file.FileName;
            var extension = Path.GetExtension(fileName);
            var Id = id + extension;
            
            if (imageName == "IdPhoto")
            {
                    
                var folderName = Path.Combine("wwwroot", "images", "tutor","IdPhotos");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id );
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }

            if (imageName == "Photo")
            {
                var folderName = Path.Combine("wwwroot", "images", "tutor","Photos");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }
                
            //student photo
            if (imageName == "image")
            {
                var folderName = Path.Combine("wwwroot", "images", "learner","Photos");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }

            if (imageName == "ABRSM")
            {
                var folderName = Path.Combine("wwwroot", "images","learner" ,"ABRSM_Grade5_Certificate");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }

            if (imageName == "CV")
            {
                var folderName = Path.Combine("wwwroot", "images","tutor" ,"CV");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }

            if (imageName == "Form")
            {
                var folderName = Path.Combine("wwwroot", "images","tutor" ,"Form");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }

            if (imageName == "Other")
            {
                var folderName = Path.Combine("wwwroot", "images","tutor" ,"Other");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, Id);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }
        }
    }
}