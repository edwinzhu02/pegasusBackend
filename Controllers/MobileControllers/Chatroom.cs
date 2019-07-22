using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Security;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers.MobileControllers
{
    public class Chatroom : Hub
    {
//        public override Task OnConnectedAsync()
//        {
//            var username = Context.UserIdentifier.
//            return base.OnConnectedAsync();
//        }

        public async Task SendMessage(string groupId, string message)
        {
            // receive the message from the client and then broadcast that same message to
            // all the clients that listen on the Broadcastchatdata event
//            var message = new ChatMessageModel
//            {
//                Name = name,
//                Message = text,
//                SendAt = DateTime.Now
//            };

//            await Clients.All.SendAsync("SendMessage", groupId, message);

            // add user into group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            await Clients.Group(groupId).SendAsync("SendMessage", $"Hello {groupId}");

        }

        // name of receiver
        public async Task<string> SendMessageOneToOne(ChatMessageModel chatMessageModel)
        {
//            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
//            var returnedMessageTime = TimeZoneInfo.ConvertTimeFromUtc(chatMessageModel.CreateAt ?? DateTime.Now, timeInfo);

            //var connectionId = Context.ConnectionId;
            var returnedMessageTime = chatMessageModel.CreateAt ?? DateTime.Now;
            try
            {
                await Clients.User(chatMessageModel.MessageBody)
                    .SendAsync("SendMessageOneToOne", chatMessageModel.SenderUserId, chatMessageModel.MessageBody,
                        returnedMessageTime.ToString("G"));
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Message send";
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }
    }
}