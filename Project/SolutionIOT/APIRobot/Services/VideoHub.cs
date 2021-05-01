using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace APIRobot.Services
{
    public class VideoHub : Hub
    {

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            var userId = this.GetIdUser();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("Disconnect");
            var userId = this.GetIdUser();
            await base.OnDisconnectedAsync(exception);
        }

        public string GetIdUser()
        {
            return this.Context.UserIdentifier;
        }

        public string GetIdWsUser()
        {
            return this.Context.ConnectionId;
        }
    }
}
