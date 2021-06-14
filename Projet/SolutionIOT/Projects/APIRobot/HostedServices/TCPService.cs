using APIRobot.Configs.HostedServices;
using APIRobot.HostedServices.HelperTCP;
using APIRobot.Models;
using APIRobot.Models.Auth;
using APIRobot.Services;
using APIRobot.Services.Cache;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace APIRobot.HostedServices
{
    public class TCPService : IHostedService, IDisposable
    {

        public class StateObjectTCP : IStateBuffer
        {
            public const int BufferReceiverSize = 2621440;

            public string Id { get; set; }
            public string IdEquipment { get; set; }
            public bool IsAuth { get; set; }
            public Socket WorkSocket { get; set; }
            public byte[] BufferReceiver { get; set; }

            public int CurrentSizeAttemping { get; set; }
            public int CurrentSizeBuffer { get; set; }
            public byte[] Buffer { get; set; }
            public byte[] Temp { get; set; }

            public StateObjectTCP()
            {
                IsAuth = false;
                BufferReceiver = new byte[BufferReceiverSize];
            }
        }

        //Server socket
        private Socket Listener;

        //Configuration
        private readonly IOptions<TCPServiceConfig> Config;

        //Logger
        private readonly ILogger<TCPService> Logger;

        //Synchronisation
        private readonly ManualResetEvent AllDone = new(false);

        //Cache clients
        private readonly Dictionary<string, StateObjectTCP> Clients;
        private readonly ITCPConnectionCache ConnectionCache;

        //Services
        private readonly IVideoQueueHandler Queue;
        private readonly IAuthorizationTCP Authorization;
        private readonly System.Timers.Timer Timer;

        public TCPService(IVideoQueueHandler queue, ITCPConnectionCache connectionCache, IAuthorizationTCP authorization, IOptions<TCPServiceConfig> config, ILogger<TCPService> logger)
        {
            Authorization = authorization;
            Queue = queue;
            Config = config;
            Logger = logger;
            ConnectionCache = connectionCache;
            Clients = new Dictionary<string, StateObjectTCP>();
            Timer = new System.Timers.Timer(5000);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            //Timer.Elapsed += VerifyConnection;
           // Timer.Start();

            return Task.Run(() => ProcessServer(cancellationToken), cancellationToken);
        }

        private void VerifyConnection(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Verifi connection");

            foreach(StateObjectTCP client in Clients.Values)
            {
                if (client.WorkSocket.Poll(5000, SelectMode.SelectRead) && client.WorkSocket.Available == 0)
                {
                    Console.WriteLine("Disconnect");
                }
                else
                {
                    Console.WriteLine("Connected");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Clients.Values
                .ToList()
                .ForEach(state => DisconnectClient(state));

            Listener.Close();

            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        private Task WaitOneAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => AllDone.WaitOne(), cancellationToken);
        }

        #region HandleConnection
        private async void ProcessServer(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Start TCP Server.");

            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new(ipAddress, Config.Value.Port);

            // Create a TCP/IP socket.  
            Listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Listener.Bind(localEndPoint);
            Listener.Listen(100);

            for (; ; )
            {
                // Set the event to nonsignaled state.  
                AllDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Logger.LogInformation("Waiting for a new connection.");
                Listener.BeginAccept(new AsyncCallback(AcceptCallback), Listener);

                // Wait until a connection is made before continuing.  
                await WaitOneAsync(cancellationToken);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            handler.NoDelay = true;
            handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            StateObjectTCP state = new()
            {
                WorkSocket = handler
            };

            StartReceiving(state);

            // Signal the main thread to continue.  
            AllDone.Set();
        }

        private void DisconnectClient(StateObjectTCP state)
        {

            if (state != null && state.Id != null && Clients.ContainsKey(state.Id))
            {
                Queue.Delete(state.IdEquipment);
                Clients.Remove(state.Id);
                ConnectionCache.DisconnectedTCP(state.Id);
                Logger.LogInformation($"Disconnect client authenticated, ID : {state.Id}");
            }
            else
                Logger.LogInformation("Disconnect client not authenticated.");

            if (state != null && state.WorkSocket.Connected)
            {
                state.WorkSocket.Shutdown(SocketShutdown.Both);
                state.WorkSocket.Close();
            }
        }

        private void DisconnectClientWithIdEquipment(string id)
        {
            if (id != null && Clients.Any(pair => pair.Value.IdEquipment.Equals(id)))
            {
                var state = Clients.FirstOrDefault(pair => pair.Value.IdEquipment.Equals(id)).Value;
                DisconnectClient(state);
            }
        }
        #endregion HandleConnection

        #region Receiving
        private void StartReceiving(StateObjectTCP state)
        {
            try
            {
                state.WorkSocket.BeginReceive(state.BufferReceiver, 0, StateObjectTCP.BufferReceiverSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (ObjectDisposedException e)
            {
                Logger.LogInformation($"Exception during begin receive : {e.Message}");
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            StateObjectTCP state = (StateObjectTCP)ar.AsyncState;
            Socket handler = state.WorkSocket;
            int bytesRead;

            try
            {
                if ((bytesRead = handler.EndReceive(ar)) > 0)
                {
                    if (state.IsAuth)
                        ProcessEnqueue(state, bytesRead);
                    else
                        ProcessAuth(state, bytesRead);
                }
                else
                    DisconnectClient(state);
            }
            catch (SocketException s)
            {
                Logger.LogInformation($"Socket disconnected because : {s.Message}.");
                DisconnectClient(state);
            }
            catch (ObjectDisposedException e)
            {
                Logger.LogInformation($"Socket disconnected because : {e.Message}.");
                DisconnectClient(state);
            }
        }
        #endregion Receiving

        #region Sending
        private void Send(Socket handler, byte[] data)
        {
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Logger.LogInformation(e.Message);
            }
        }
        #endregion Sending

        #region EnqueueData
        private void ProcessEnqueue(StateObjectTCP state, int bytesRead)
        {
            byte[] buffer = new byte[bytesRead];

            Buffer.BlockCopy(state.BufferReceiver, 0, buffer, 0, bytesRead);
            Array.Clear(state.BufferReceiver, 0, StateObjectTCP.BufferReceiverSize);

            ProcessEnqueue(state, buffer);

        }
        private void ProcessEnqueue(StateObjectTCP state, byte[] bytes)
        {

            if (!this.Queue.EnQueue(state.IdEquipment,bytes))
                DisconnectClient(state);
            else
                StartReceiving(state);
        }
        #endregion  EnqueueData

        #region Auth
        private void VerifyAuth(StateObjectTCP state, byte[] data)
        {
            string jwt = Encoding.UTF8.GetString(data);
            var canConnect = Authorization.VerifyTCPAuthorizationConnection(jwt, out EquipmentIdentity identity);

            //if have not identity decodes with jwt token, disconnect client
            if(identity is null || !canConnect)
            {
                Logger.LogInformation($"Failure authentification client : {state.Id}.");
                DisconnectClient(state);
                return;
            }
            else
            {
                //if have already client with the same id equipment
                if (Clients.Any(pair => pair.Value.IdEquipment.Equals(identity.IdEquipment)))
                    DisconnectClientWithIdEquipment(identity.IdEquipment);

                //populate data auth for state tcp
                state.Id = Guid.NewGuid().ToString();
                state.IsAuth = true;
                state.IdEquipment = identity.IdEquipment;

                //add new equipment to cache 
                ConnectionCache.ConnectedTCP(identity, state.Id, ((IPEndPoint)state.WorkSocket.RemoteEndPoint).Address.ToString());

                Clients.Add(state.Id, state);

                Logger.LogInformation($"Succesfully authentification client : {state.Id}.");
            }
        }

        private void ProcessAuth(StateObjectTCP state, int bytesRead)
        {
            byte[] buffer = new byte[bytesRead];

            Buffer.BlockCopy(state.BufferReceiver, 0, buffer, 0, bytesRead);
            Array.Clear(state.BufferReceiver, 0, StateObjectTCP.BufferReceiverSize);

            byte[] rest = HelperParser.ProcessBytes(
                ref buffer, 
                state, 
                () => DisconnectClient(state), 
                (data) => VerifyAuth(state, data)
            );

            //if the socket is already connected 
            if (state.WorkSocket.Connected)
            {
                //if is authentificate after redirige rest of data else continue to receive data auth
                if(state.IsAuth && rest is not null)
                    ProcessEnqueue(state, rest);
                else
                    StartReceiving(state);
            }
        }
        #endregion Auth
    }
}
