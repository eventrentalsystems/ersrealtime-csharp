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
    class RealtimePubSub
    {

        private Dictionary<string, HashSet<Action<int>>> channels;
        private readonly ProtocolClient pclient;

        public RealtimePubSub()
        {
            channels = new Dictionary<string, HashSet<Action<int>>>();
            pclient = new ProtocolClient();
            pclient.SetMessageHandler((string foldername, string topic, int payload) =>
                HandleMessage(foldername, topic, payload));
        }

        private void HandleMessage(string foldername, string topic, int payload)
        {
            foreach (var listener in channels[$"{foldername}:{topic}"])
            {
                listener(payload);
            }
        }

        public void Subscribe(string foldername, string topic, Action<int> listener)
        {
            string channel = $"{foldername}:{topic}";
            if (!channels.ContainsKey(channel))
            {
                pclient.SendSubscribe(foldername, topic);
                channels.Add(channel, new HashSet<Action<int>>());
            }
            channels[channel].Add(listener);
        }

        public void Unsubscribe(string foldername, string topic, Action<int> listener)
        {
            string channel = $"{foldername}:{topic}";
            if (channels.ContainsKey(channel))
            {
                channels[channel].Remove(listener);
                if (channels[channel].Count == 0)
                {
                    pclient.SendUnsubscribe(foldername, topic);
                    channels.Remove(channel);
                }
            }
        }
    }
}