using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientUDP
{
    class ClientUDP
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Thread thdUDPServer = new Thread(new ThreadStart(ReceiverThread));
            thdUDPServer.Start();
            
 
        }

        private static void ClientThread()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse("172.20.10.11"), 10000);
            Byte[] senddata = Encoding.ASCII.GetBytes("Hello World");
            udpClient.Send(senddata, senddata.Length);
        }

        private static void ReceiverThread()
        {
            UdpClient udpClient = new UdpClient(10000);
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());
                if (returnData.ToString().Equals("GETDATA"))
                {
                    Thread thdUDPServer = new Thread(new ThreadStart(ClientThread));
                    thdUDPServer.Start();
                }
            }
        }


    }


}
