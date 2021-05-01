using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace APIRobot.Services
{
    public class UDPService : IHostedService, IDisposable
    {

        class StateObjectUDP
        {
            // Size of receive buffer.  
            public const int BufferSize = 65535;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
        }

        private Socket Socket;
        private EndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, 11000);
        private readonly ManualResetEvent AllDone = new(false);
        private readonly VideoQueue Queue;

        public UDPService(VideoQueue queue)
        {
            this.Queue = queue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => ProcessServer(cancellationToken), cancellationToken);
        }

        private void ProcessServer(CancellationToken cancellationToken)
        {

            Socket = new Socket(LocalEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            AsyncCallback callback = new(ReadCallback);
            StateObjectUDP state = new();

            for (; ; )
            {
                AllDone.Reset();
                Socket.BeginReceiveFrom(state.buffer, 0, StateObjectUDP.BufferSize, SocketFlags.None, ref LocalEndPoint, callback, state);
                AllDone.WaitOne();
            }

        }

        private void ReadCallback(IAsyncResult ar)
        {

            StateObjectUDP state = (StateObjectUDP)ar.AsyncState;

            // Read data from the client socket.
            int bytesRead = Socket.EndReceiveFrom(ar, ref LocalEndPoint); 

            if (bytesRead > 0)
            {

                byte[] buffer = new byte[bytesRead];

                //Copy and clear bytes
                Buffer.BlockCopy(state.buffer, 0, buffer, 0, bytesRead);
                Array.Clear(state.buffer, 0, StateObjectUDP.BufferSize);

                this.Queue.EnQueue(buffer);

                AllDone.Set();
            }
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
