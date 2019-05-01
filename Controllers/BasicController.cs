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
namespace Pegasus_backend.Controllers
{
    public abstract class BasicController: ControllerBase
    {

        protected IEnumerable<NavItem> GetNavItems(string roleName)
        {
            
            switch (roleName)
            {
                case "Receptionist":
                    return new List<NavItem>
                    {
                        new NavItem{pagename = "Home",pageicon = "fa-home",pagelink = "testcontent"},
                        new NavItem{pagename = "Registration",pageicon = "fa-th",pagelink = "registration"},
                        new NavItem{pagename = "Payment",pageicon = "fa-chart-bar",pagelink = "payment"},
                        new NavItem{pagename = "Forms",pageicon = "fa-clipboard",pagelink = ""}
                    };
                    
                //need to change later
                default:
                    return null;
                    
            }
        }

        protected bool IsNull<T>(T item)
        {
            if (item == null)
            {
                return true;
            }

            return false;
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
        public void UploadFile(IFormFile file,string imageName)
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