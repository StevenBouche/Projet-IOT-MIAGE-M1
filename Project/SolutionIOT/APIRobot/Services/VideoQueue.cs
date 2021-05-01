using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APIRobot.Services
{
    public class VideoQueue : IHostedService, IDisposable
    {

        private readonly ConcurrentQueue<byte[]> Queue = new();
        private readonly ManualResetEvent AllDone = new(false);
        private readonly IHubContext<VideoHub> HubContext;

        public VideoQueue(IHubContext<VideoHub> hubContext)
        {
            this.HubContext = hubContext;
        }

        public void EnQueue(byte[] bytes)
        {
            Queue.Enqueue(bytes);
            this.AllDone.Set();
        }

        public bool Dequeue(out byte[] bytes)
        {
            return Queue.TryDequeue(out bytes);
        }


        public void ProcessQueue(CancellationToken cancellationToken)
        {
            for(; ; )
            {
                AllDone.Reset();

                while(Queue.TryDequeue(out byte[] bytes))
                {
                    HubContext.Clients.All.SendAsync("stream", bytes, cancellationToken);
                }

                AllDone.WaitOne();
            }
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
