using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KinectCoordinateMapping.Model;

namespace KinectCoordinateMapping.Manager
{
    enum State
    {
        SendingData,
        WaitStartData
    }

    class NetworkController
    {
        private int _portEcoute;
        private Boolean running;
        private Thread _UDPServer;
        private Thread _SendDataBody;
        private BodyHandler _bodyHandler;
        private Boolean runningSendData;
        private Boolean runningMulticast;
        private State State;

        public NetworkController(int portEcoute, BodyHandler bodyHandler)
        {
            _portEcoute = portEcoute;
            _bodyHandler = bodyHandler;
            runningSendData = false;
            runningMulticast = false;
            State = State.WaitStartData;
        }

        public void StartUDP()
        {
            running = true;
            _SendDataBody = null;
            _UDPServer = new Thread(new ThreadStart(ReceiverThread));
            _UDPServer.Start();
            
            Console.WriteLine("NetworkController : Service UDP Start");
        }

        private void SendUDP(string addrIP, int port, string message)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(addrIP), 10000);
            Byte[] senddata = Encoding.UTF8.GetBytes(message);
            udpClient.Send(senddata, senddata.Length);
        }
        
        private void SendMulticastUDP(UdpClient udpClient, string message)
        {
            Byte[] senddata = Encoding.UTF8.GetBytes(message);
            udpClient.Send(senddata, senddata.Length);
        }

        private void SendDataBody(string addrIp, int port)
        {
            runningSendData = true;
            State = State.SendingData;
            string message;
            while (runningSendData)
            {
                string left = _bodyHandler.GetVariationLeft().ToString(CultureInfo.InvariantCulture);
                string right = _bodyHandler.GetVariationRight().ToString(CultureInfo.InvariantCulture);
                string Max = _bodyHandler.GetVariationMax().ToString(CultureInfo.InvariantCulture);
                string Min = _bodyHandler.GetVariationMin().ToString(CultureInfo.InvariantCulture);
                int hand = (_bodyHandler.GetRightHandOpen() && _bodyHandler.GetRightHandOpen()) ? 1 : 0;
                message = "DATA:"+right+":"+left+":"+Min+":"+Max+":"+hand+":END";
                SendUDP(addrIp, port, message);
                Thread.Sleep(300);
            }
        }

        private void ReceiverThread()
        {
            
            try
            {
                // Code that is executing when the thread is aborted.  
                UdpClient udpClient = new UdpClient(_portEcoute);
                while (running)
                {
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    Console.WriteLine(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());
                    if (returnData.ToString().Equals("GETDATABODY"))
                    {
                        if (!runningSendData)
                        {
                            _SendDataBody = new Thread(() => SendDataBody(RemoteIpEndPoint.Address.ToString(), RemoteIpEndPoint.Port));
                            _SendDataBody.Start();
                        }
                    }
                    else if (returnData.ToString().Equals("STOPDATABODY"))
                    {
                        StopUDP();
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                // Clean-up code can go here.  
                // If there is no Finally clause, ThreadAbortException is  
                // re-thrown by the system at the end of the Catch clause. 
               
            }
            finally
            {
                running = false;
            }

        }
        
        private void ReceiverMulticastThread()
        {
            
            try
            {
                // Code that is executing when the thread is aborted.  
                UdpClient udpClient = new UdpClient();
                IPAddress multiUDP = IPAddress.Parse("224.7.7.7");
                udpClient.JoinMulticastGroup(multiUDP,7777);
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(multiUDP, 7777);
                
                while (runningMulticast)
                {
                    
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    Console.WriteLine(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());
                    if (returnData.ToString().Equals("SYNC:"))
                    {
                        if (!runningSendData)
                        {
                            _SendDataBody = new Thread(() => SendMulticastUDP(udpClient,"SYNC:OK"));
                            _SendDataBody.Start();
                        }
                    }
                    else if (returnData.ToString().Equals("STOPDATABODY"))
                    {
                        StopUDP();
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                // Clean-up code can go here.  
                // If there is no Finally clause, ThreadAbortException is  
                // re-thrown by the system at the end of the Catch clause. 
               
            }
            finally
            {
                runningMulticast = false;
            }

        }

        public void StopUDP()
        {
            State = State.WaitStartData;
            runningSendData = false;
            _UDPServer.Abort();
            Console.WriteLine("NetworkController : Service UDP Stop");
        }

       public bool GetRunning()
        {
           return this.running;
        }

        public string GetStateNetwork()
        {
            return this.State.ToString();
        }
    }
}
