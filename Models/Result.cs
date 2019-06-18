using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pegasus_backend.Models
{
    public class Result<T>
    {
        //1--Success
        public bool IsSuccess { get; set; } = true;
        public string ErrorCode { get; set; }
        public bool IsFound { get; set; } = true;
        public string ErrorMessage { get; set; }
        public string Note { get; set; }
        public T Data { get; set; }
        
        
    }
}