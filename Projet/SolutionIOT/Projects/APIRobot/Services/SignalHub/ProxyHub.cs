using APIRobot.Models;
using APIRobot.Models.Data;
using APIRobot.SignalHub;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIRobot.Services.SignalHub
{
    public interface IProxyHubEquipment
    {
        Task OnChangeEquipment(EquipmentConnection data);
        Task OnNewDataRobot(DataRobot data);
        Task OnChangeEquipments(List<EquipmentConnection> data);
    }

    public class ProxyHub : IProxyHubEquipment
    {

        private readonly IHubContext<EquipmentHub> EquipmentHub;
        private readonly IHubContext<EquipmentsHub> EquipmentsHub;

        public ProxyHub(IHubContext<EquipmentHub> equipmentHub, IHubContext<EquipmentsHub> equipmentsHub)
        {
            EquipmentHub = equipmentHub;
            EquipmentsHub = equipmentsHub;
        }

        public Task OnChangeEquipment(EquipmentConnection data)
        {
            return EquipmentHub.Clients.Group(data.IdEquipment).SendAsync("onChangeEquipment", data);
        }

        public Task OnChangeEquipments(List<EquipmentConnection> data)
        {
            return EquipmentsHub.Clients.All.SendAsync("onChangeEquipments", data);
        }

        public Task OnNewDataRobot(DataRobot data)
        {
            return EquipmentHub.Clients.Group(data.IdESP).SendAsync("onDataEquipment", data);
        }
    }
}
