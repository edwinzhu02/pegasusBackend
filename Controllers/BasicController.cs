using Pegasus_backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public void UploadFile(string fileName,IFormFile file,string folderName )
        {
            try
            {
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var path = Path.Combine(pathToSave, fileName);
                var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}