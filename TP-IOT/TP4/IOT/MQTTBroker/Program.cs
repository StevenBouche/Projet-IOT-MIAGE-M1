using MQTTBroker.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTBroker
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://www.codeproject.com/Articles/5283088/MQTT-Message-Queue-Telemetry-Transport-Protocol-wi

            MQTTServer server = new("127.0.0.1", 1883);
            // MQTTClient client = new("client1", "127.0.0.1", 1883);
            //MQTTClient client2 = new("client2", "127.0.0.1", 1883);

            server.StartBroker();

            /* client.Connect();
             client2.Connect();

             client.Subscribe("test");
             client.Subscribe("test2");

             client2.Subscribe("test");
             client2.Subscribe("test2");

             client.Publish("test", "hello im the first client");
             client2.Publish("test2", "hello im the second client");

             client.Unsubscribe("test2");

             client2.Publish("test2", "hello im the second client x 2");*/

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

            /*  client.Disconnect();
              client2.Disconnect();*/

            //To keep the app running in container
            //https://stackoverflow.com/questions/38549006/docker-container-exits-immediately-even-with-console-readline-in-a-net-core-c
            Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();

            server.StopBroker();

        }
    }
}
