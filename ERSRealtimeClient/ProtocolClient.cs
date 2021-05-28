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
    class ProtocolClient
    {
        private IWebsocketClient client;
        private Action<string, string, int> listener;

        public ProtocolClient()
        {
            var url = new Uri("wss://ersrealtime.com");
            var factory = new Func<ClientWebSocket>(() =>
            {
                var client = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(5),
                    }
                };
                return client;
            });

            client = new WebsocketClient(url, factory);
            client.Name = "ERS Realtime";
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);

            client.ReconnectionHappened.Subscribe(type => HandleReconnect(type));
            client.DisconnectionHappened.Subscribe(info => HandleDisconnect(info));
            client.MessageReceived.Subscribe(msg => HandleReception(this, msg));
            client.Start();
        }

        public void SendSubscribe(string foldername, string topic)
        {
            // consider if (client.IsRunning) ...
            client.Send($"[\"subscribe\", \"{foldername}:{topic}\"]");
        }

        public void SendUnsubscribe(string foldername, string topic)
        {
            // consider if (client.IsRunning) ...
            client.Send($"[\"unsubscribe\", \"{foldername}:{topic}\"]");
        }

        private void HandleDisconnect(DisconnectionInfo info)
        {
            // TODO - add way to listen for this event.
            // Console.WriteLine($"Disconnected, type {info.Type}");
        }

        private void HandleReconnect(Websocket.Client.Models.ReconnectionInfo type)
        {
            // TODO - add way to listen for this event.
            // Console.WriteLine($"Reconnected, type: {type}, url: {client.Url}");
        }

        private static void HandleReception(ProtocolClient self, ResponseMessage msg)
        {
            string raw = $"{msg}";
            string[] components = raw.Split(':');
            string foldername = components[0];
            string topic = components[1];
            string raw_payload = components[2];
            int payload = Int32.Parse(raw_payload);

            self.NotifyListener(foldername, topic, payload);
            Console.WriteLine($"message recvd: {foldername} - {topic} - {payload}.");
        }

        public void NotifyListener(string foldername, string topic, int payload)
        {
            listener(foldername, topic, payload);
        }

        public void SetMessageHandler(Action<string, string, int> messageListener)
        {
            listener = messageListener;
        }

    }
}
