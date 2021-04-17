using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerUDP
{
    class ServerUDP
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Thread thdUDPServer = new Thread(new ThreadStart(ReceiverThread));
            thdUDPServer.Start();

            ClientThread("Hello Kinect it's robot lego mindstorm wait sec");
            ClientThread(".");
            Thread.Sleep(2000);
            ClientThread("..");
            Thread.Sleep(2000);
            ClientThread("...");
            Thread.Sleep(2000);
            ClientThread("GETDATABODY");
 
        }

        private static void ClientThread(string msg)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse("172.20.10.5"), 10000);
            Byte[] senddata = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(senddata, senddata.Length);
        }

        private static void ReceiverThread()
        {
            UdpClient udpClient = new UdpClient(10001);
            while (true)
            {
              /*  Thread thdUDPServer = new Thread(new ThreadStart(ClientThread));
                thdUDPServer.Start();*/
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.UTF8.GetString(receiveBytes);
                Console.WriteLine(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());
            }
        }

     
    }


}
