using APIRobot.Models.Data;
using APIRobot.Services.Cache;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace APIRobot.Services.SignalHub
{
    public class EquipmentStreamHub : Hub
    {
        private readonly IChannelConnectionCache ChannelHandler;
        private readonly ILogger<EquipmentStreamHub> Logger;

        public EquipmentStreamHub(IChannelConnectionCache channelHandler, ILogger<EquipmentStreamHub> logger)
        {
            Logger = logger;
            ChannelHandler = channelHandler;
        }

        public ChannelReader<string> EquipmentStream(string idEquipment)
        {
            return ChannelHandler.GetChannelsEquipment(idEquipment)?.Stream.Reader;
        }

        public ChannelReader<MotorValues> EquipmentStreamMotor(string idEquipment)
        {
            return ChannelHandler.GetChannelsEquipment(idEquipment)?.StreamMotor.Reader;
        }

    }
}
