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


        protected bool IsNull<T>(T item)
        {
            if (item == null)
            {
                return true;
            }

            return false;
        }

        protected Object UserInfoFilter(User user)
        {
            if (user.Teacher.Count != 0)
            {
               return new {firstname=user.Teacher.ToList()[0].FirstName,lastname=user.Teacher.ToList()[0].LastName};
            }

            if (user.Staff.Count != 0)
            {
                return new {firstname=user.Staff.ToList()[0].FirstName,lastname=user.Staff.ToList()[0].LastName};
            }

            if (user.Learner.Count != 0)
            {
                return new {firstname=user.Learner.ToList()[0].FirstName,lastname=user.Learner.ToList()[0].LastName};
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
        protected void UploadFile(IFormFile file,string imageName)
        {
            try
            {
                if (imageName == "IdPhoto")
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var folderName = Path.Combine("wwwroot", "images", "TeacherIdPhotos");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var path = Path.Combine(pathToSave, fileName);
                    var stream = new FileStream(path, FileMode.Create);
                    file.CopyTo(stream);
                    stream.Close();
                }

                if (imageName == "Photo")
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var folderName = Path.Combine("wwwroot", "images", "TeacherImages");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var path = Path.Combine(pathToSave, fileName);
                    var stream = new FileStream(path, FileMode.Create);
                    file.CopyTo(stream);
                    stream.Close();
                }
                
                //student photo
                if (imageName == "image")
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var folderName = Path.Combine("wwwroot", "images", "LearnerImages");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var path = Path.Combine(pathToSave, fileName);
                    var stream = new FileStream(path, FileMode.Create);
                    file.CopyTo(stream);
                    stream.Close();
                }

                if (imageName == "ABRSM")
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var folderName = Path.Combine("wwwroot", "images", "ABRSM_Grade5_Certificate");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var path = Path.Combine(pathToSave, fileName);
                    var stream = new FileStream(path, FileMode.Create);
                    file.CopyTo(stream);
                    stream.Close();
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}