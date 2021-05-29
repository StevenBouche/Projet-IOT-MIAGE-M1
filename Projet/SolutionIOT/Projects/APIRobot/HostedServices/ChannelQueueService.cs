using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Collections.Generic;
using APIRobot.HostedServices.HelperTCP;
using APIRobot.Models.Data;
using APIRobot.Services.Cache;

namespace APIRobot.HostedServices
{

    public interface IVideoQueueHandler
    {
        bool EnQueue(string id, byte[] bytes);
        void Delete(string idEquipment);
    }

    /*public interface IVideoChannelHandler
    {
        ChannelReader<string> GetChannelReader(string idChannel);
    }*/

   /* public interface IMotorQueueHandler
    {
        void EnQueue(string id, MotorValues value);
    }

    public interface IMotorChannelHandler
    {
        ChannelReader<MotorValues> GetChannelReader(string idChannel);
    }*/

    public class ChannelQueueService : IHostedService, IDisposable, IVideoQueueHandler /*IVideoChannelHandler, IMotorQueueHandler, IMotorChannelHandler*/
    {

        class StateProcess : IStateBuffer
        {
            public int CurrentSizeAttemping { get; set; }
            public int CurrentSizeBuffer { get; set; }
            public byte[] Buffer { get; set; }
            public byte[] Temp { get; set; }

            public StateProcess()
            {
                CurrentSizeAttemping = 0;
                CurrentSizeBuffer = 0;
            }
        }

        class DataQueue
        {
            public string IdEquipment { get; set; }
            public readonly ConcurrentQueue<byte[]> Queue = new();
            public bool DeSync = false;
            public StateProcess State = new();
            public string CurrentDataURI = "";
        }

        private const string BaseURL = @"data:jpg;base64, {0}";

        //Syncronisation
        private readonly ManualResetEvent AllDone = new(false);
        //Cache
        private readonly ConcurrentDictionary<string, DataQueue> DictionnaryQueue;

        private readonly IChannelConnectionCache CacheChannel;

        public ChannelQueueService(IChannelConnectionCache cacheChannel)
        {
            CacheChannel = cacheChannel;
            DictionnaryQueue = new ConcurrentDictionary<string, DataQueue>();
        }

       /* public ChannelReader<string> GetChannelReader(string idChannel)
        {
            DictionnaryQueue.TryGetValue(idChannel, out DataQueue queue);

            if (queue is null)
                return null;

            return queue.Stream.Reader;
        }*/

       /* ChannelReader<MotorValues> IMotorChannelHandler.GetChannelReader(string idChannel)
        {
            DictionnaryQueue.TryGetValue(idChannel, out DataQueue queue);

            if (queue is null)
                return null;

            return queue.StreamMotor.Reader;
        }*/

        public bool EnQueue(string id, byte[] bytes)
        {

            DictionnaryQueue.TryGetValue(id, out DataQueue queue);

            if(queue is null)
            {
                queue = new DataQueue() { 
                    IdEquipment = id
                };
                DictionnaryQueue.TryAdd(id, queue);
            }

            queue.Queue.Enqueue(bytes);
            AllDone.Set();

            return !queue.DeSync;
        }

        /*public void EnQueue(string id, MotorValues value)
        {
            DictionnaryQueue.TryGetValue(id, out DataQueue queue);

            if (queue is not null)
            {
                queue = new DataQueue();
                DictionnaryQueue.TryAdd(id, queue);
            }

            Task.Run(() => queue.StreamMotor.Writer.WriteAsync(value));
        }*/

        public void Delete(string id)
        {
            DictionnaryQueue.TryRemove(id, out DataQueue queue);

            //if (queue is not null)
                //queue.Stream.Writer.Complete();
        }

        private void SetNewData(DataQueue queue, byte[] bytes, CancellationToken cancellationToken)
        {
            ChannelConnection channels = CacheChannel.GetChannelsEquipment(queue.IdEquipment);
            queue.CurrentDataURI = string.Format(BaseURL, Convert.ToBase64String(bytes));
            Task.Run(() => channels?.Stream.Writer.WriteAsync(queue.CurrentDataURI, cancellationToken), cancellationToken);
        }

        private void ProcessBytes(DataQueue queue, ref byte[] bytes, CancellationToken cancellationToken)
        {
            byte[] rest = HelperParser.ProcessBytes(ref bytes, queue.State, () => queue.DeSync = true, (buffer) => SetNewData(queue, buffer, cancellationToken));

            if(rest != null)
                ProcessBytes(queue, ref rest, cancellationToken);
        }

        private async void ProcessQueue(CancellationToken cancellationToken)
        {

            for (; ; )
            {
                AllDone.Reset();

                foreach(KeyValuePair<string, DataQueue> pair in DictionnaryQueue)
                {
                    while (pair.Value.Queue.TryDequeue(out byte[] bytes))
                    {
                        if (!pair.Value.DeSync)
                        {
                            ProcessBytes(pair.Value, ref bytes, cancellationToken);
                        }
                    }
                }
                await WaitOneAsync();
            }
        }

        private Task WaitOneAsync()
        {
            return Task.Run(() => AllDone.WaitOne());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => ProcessQueue(cancellationToken), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }
    }
}
