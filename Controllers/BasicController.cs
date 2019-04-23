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