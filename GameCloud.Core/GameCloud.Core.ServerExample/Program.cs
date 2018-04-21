using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameCloud.Core.ServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();

            while (true)
            {
                Thread.Sleep(10);
            }
        }

        static async Task MainAsync()
        {
            await Task.Delay(100);
//
//            // ------------------------------------
//            // START THE MAIN SERVER
//
//            var mainServer = new Server(500).Start();
//
//            // ------------------------------------
//            // START THE CONNECTORS
//
//            var connector1 = new Server(501).Start();
//            var connector2 = new Server(502).Start();
//            var connector3 = new Server(503).Start();
//
//            // Default relay
//            await connector1.RelayTo("127.0.0.1", mainServer.Port);
//            await connector2.RelayTo(1, "127.0.0.1", mainServer.Port);
//            await connector3.RelayTo(2, 5, "127.0.0.1", mainServer.Port);

        }
    }
}
