using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
//using System.Web.Helpers;
//using System.Web.Http.Controllers;
using Pegasus_backend.Models;
//using System.Web.Http.Filters;
//using System.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pegasus_backend.ActionFilter
{
    public class CheckModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var check = CheckStateModel(filterContext.ModelState);

            //JObject checkResult = JObject.Parse(JsonConvert.SerializeObject(check));

            if (check.IsSuccess == false)
            {
                filterContext.Result = new BadRequestObjectResult(filterContext.ModelState);
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }

        }

        protected Result<string> CheckStateModel(ModelStateDictionary modelState)
        {
            var result = new Result<string>();
            if (!modelState.IsValid)
            {
                result.IsSuccess = false;
                result.ErrorMessage = string.Join(",", GetErrorMessages(modelState));
                return result;
            }
            return result;
        }
        protected string[] GetErrorMessages(ModelStateDictionary modelStates)
        {
            var errorMessages = new List<string>();
            foreach (var modelState in modelStates.Values)
            {
                foreach (var modelError in modelState.Errors)
                {
                    errorMessages.Add(modelError.ErrorMessage);
                }
            }
            return errorMessages.ToArray();
        }
    }
}