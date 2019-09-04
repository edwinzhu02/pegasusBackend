using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Security;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers.MobileControllers
{
    public class Chatroom : Hub
    {
        private readonly ablemusicContext _ablemusicContext;
        private int _userId;
        public Chatroom (ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            _userId = int.Parse(httpContext.Request.Query["userId"]);
            // update user status to online
            User userConnected = await _ablemusicContext.User.Where(x => x.UserId == _userId).FirstOrDefaultAsync();
            if (userConnected == null)
            {
                throw new HubException("UserId connected can't be found.");
            }
            userConnected.IsOnline = 1;
            await _ablemusicContext.SaveChangesAsync();
            List<User> connectedUsers =
                await _ablemusicContext.User.Where(x => x.IsOnline == 1 && x.IsActivate == 1).ToListAsync();
            await Clients.Others.SendAsync("OnlineUserUpdate", _userId + "is now online.");
            await Clients.All.SendAsync("OnlineUserList", JsonConvert.SerializeObject(connectedUsers));
            await base.OnConnectedAsync();
        }

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
//            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
//            await Clients.Group(groupId).SendAsync("SendMessage", $"Hello {groupId}");

        }


        public async Task<string> SendMessageOneToOne(ChatMessageModel chatMessageModel)
        {
//            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
//            var returnedMessageTime = TimeZoneInfo.ConvertTimeFromUtc(chatMessageModel.CreateAt ?? DateTime.Now, timeInfo);

            //var connectionId = Context.ConnectionId;
            var returnedMessageTime = chatMessageModel.CreateAt ?? DateTime.Now;
            try
            {
                var senderRoleId = _ablemusicContext.User.Where(x => x.UserId == chatMessageModel.SenderUserId)
                    .Select(x => x.RoleId).FirstOrDefault();
                if (senderRoleId == null)
                {
                    return "Cannot find userId of message sender";
                }

                var roleName = _ablemusicContext.Role.Where(x => x.RoleId == senderRoleId).Select(x => x.RoleName)
                    .First();

                await Clients.User(chatMessageModel.ReceiverUserId.ToString())
                    .SendAsync("SendMessageOneToOne", chatMessageModel.SenderUserId, chatMessageModel.MessageBody,
                        returnedMessageTime.ToString("G"), roleName);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Message send";
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            //            await base.OnDisconnectedAsync(exception);
            User userDisconnected = await _ablemusicContext.User.Where(x => x.UserId == _userId).FirstOrDefaultAsync();
            userDisconnected.IsOnline = 0;
            await _ablemusicContext.SaveChangesAsync();
            List<User> connectedUsers =
                await _ablemusicContext.User.Where(x => x.IsOnline == 1 && x.IsActivate == 1).ToListAsync();
            await Clients.Others.SendAsync("OnlineUserUpdate", _userId + "is now offline.");
            await Clients.All.SendAsync("OnlineUserList", JsonConvert.SerializeObject(connectedUsers));
            await base.OnDisconnectedAsync(exception);
        }
    }
}