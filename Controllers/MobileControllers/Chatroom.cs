using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Security;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers.MobileControllers
{
    public class Chatroom : Hub
    {

        public async Task SendMessage(string name, string message)
        {
            // receive the message from the client and then broadcast that same message to
            // all the clients that listen on the Broadcastchatdata event
//            var message = new ChatMessageModel
//            {
//                Name = name,
//                Message = text,
//                SendAt = DateTime.Now
//            };

            await Clients.All.SendAsync("SendMessage",name, message);
        }
    }

//    public interface ITypedHubClient
//    {
//        Task SendMessageToClient(string name, string message);
//    }
}