using APIRobot.Models;
using APIRobot.Services.Cache;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace APIRobot.Services.SignalHub
{
    public class EquipmentsHub : Hub
    {

        private readonly ICacheEquipment Cache;

        public EquipmentsHub(ICacheEquipment cache)
        {
            Cache = cache;
        }

        public List<EquipmentConnection> EquipmentStatus()
        {
            return Cache.GetEquipmentConnected();
        }
    }
}
