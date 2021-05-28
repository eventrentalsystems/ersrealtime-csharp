using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Websocket.Client;

namespace ERSRealtimeClient
{
    class Program
    {
        public static void Main()
        {
            var subscriber = new RealtimePubSub();

            void handler(int thing)
            {
                Console.WriteLine($"program heard someone say {thing}");
            }

            subscriber.Subscribe("default_dev", "test", handler);
            Task.Delay(1000).Wait();

            subscriber.Unsubscribe("default_dev", "test", handler);
            Task.Delay(1000).Wait();

        }
    }
}
