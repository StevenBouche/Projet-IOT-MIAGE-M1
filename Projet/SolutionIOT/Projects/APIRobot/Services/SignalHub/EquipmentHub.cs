using APIRobot.HostedServices;
using APIRobot.Models;
using APIRobot.Models.Data;
using APIRobot.Services;
using APIRobot.Services.Cache;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace APIRobot.SignalHub
{
    public class EquipmentHub : Hub
    {

        private const string IdEquipmentKey = "idEquipment";

        //Logger
        private readonly ILogger<EquipmentHub> Logger;

        //Services
        private readonly DataRobotService ServiceData;
        private readonly ICacheEquipment Cache;
     
        public EquipmentHub(ICacheEquipment cache, DataRobotService serviceData, ILogger<EquipmentHub> logger)
        {
            Cache = cache;
            Logger = logger;
            ServiceData = serviceData;
        }

        public EquipmentConnection EquipmentStatus()
        {
            var id = Context.ConnectionId;
            var idEquipment = GetIdEquipmentOfContext();

            return Cache.GetEquipmentConnected().FirstOrDefault(equipment => equipment.IdEquipment.Equals(idEquipment));
        }

        public List<DataRobot> LastDataEquipment(int size)
        {
            var idEquipment = GetIdEquipmentOfContext();
            return ServiceData.SearchLast(idEquipment, size);
        }

        public override async Task OnConnectedAsync()
        {

            var id = Context.ConnectionId;
            var idRobot = Context.GetHttpContext().Request.Query[IdEquipmentKey];

            var equipment = Cache.GetEquipmentConnected().FirstOrDefault(equipment => equipment.IdEquipment.Equals(idRobot));

            if (equipment is null)
            {
                Context.Abort();
                return;
            }

            await PushClientToGroup(idRobot, id, Context.ConnectionAborted);

            await base.OnConnectedAsync();

            Logger.LogInformation($"Client connected : {id}.");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var id = Context.ConnectionId;

            await RemoveClientFromGroups(id);

            await base.OnDisconnectedAsync(exception);

            Logger.LogInformation($"Client disconnected : {id}.");
        }

        private async Task PushClientToGroup(string groupname, string idClient, CancellationToken token)
        {
           
            await Groups.AddToGroupAsync(idClient, groupname, token);

            Context.Items.Add(IdEquipmentKey, groupname);

            Logger.LogInformation($"Client {idClient} added to group {groupname}.");
        }

        private async Task RemoveClientFromGroups(string idClient)
        {
            var idEquipment = GetIdEquipmentOfContext();

            await Groups.RemoveFromGroupAsync(idClient, idEquipment, Context.ConnectionAborted);

            Logger.LogInformation($"Client {idClient} removed from group {idEquipment}.");
        }

        private string GetIdEquipmentOfContext()
        {
            Context.Items.TryGetValue(IdEquipmentKey, out object idEquipment);
            if (idEquipment is not null && idEquipment is string)
                return idEquipment as string;
            else
                return string.Empty;
        }
    }
}
