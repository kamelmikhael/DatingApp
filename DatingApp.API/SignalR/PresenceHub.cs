using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DatingApp.API.Extensions;
using System;
using Microsoft.AspNetCore.Authorization;

namespace DatingApp.API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.GetUserName();
            
            var isOnline = await _tracker.UserConnected(userName, Context.ConnectionId);
            if(isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", userName);
            }

            var currentOnlineUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentOnlineUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.GetUserName();

            var isOffline = await _tracker.UserDisconnected(userName, Context.ConnectionId);

            if(isOffline) {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}