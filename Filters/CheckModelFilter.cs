using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pegasus_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pegasus_backend.Filters
{
    public class CheckModelFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var check = CheckStateModel(context.ModelState);

            if (!context.ModelState.IsValid)
            {
                 context.Result = new BadRequestObjectResult(check);
            }
            else
            {
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
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
