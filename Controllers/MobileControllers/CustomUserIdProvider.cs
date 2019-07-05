using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Pegasus_backend.Controllers.MobileControllers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId (HubConnectionContext  connectionContext)
        {
            var username = connectionContext.GetHttpContext().Request.Query["userId"];
            return username;
        }
    }
}

