using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BattleshipsServer
{
    public class Server
    {
        public const int Port = 7070;

        private readonly List<Player> players = new List<Player>();

        public void Start() => StartListening();

        private async void StartListening()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("Listening...");
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                if (players.Count >= 2)
                    continue; // todo

                Console.WriteLine($"Connection from {client.Client.LocalEndPoint} accepted");
                var player = new Player(client);
                players.Add(player);
                new Thread(() => HandlePlayer(player)).Start();
            }
        }

        private void HandlePlayer(Player player)
        {
            while (true)
                ParseTraffic(player, player.Reader.ReadString());
        }

        private Player OpponentOf(Player player) => players.Find(a => !a.Equals(player));

        private void ParseTraffic(Player player, string traffic)
        {
            int separatorIndex = traffic.IndexOf(":", StringComparison.Ordinal);

            if (separatorIndex == -1)
            {
                Console.WriteLine($"{player.NameWithId} sent a packet without a header:\n{traffic}");
                return;
            }

            string header = traffic.Substring(0, separatorIndex);
            string data = traffic.Substring(separatorIndex + 1);
            
            switch (header)
            {
                case "name":
                    player.Name = data;
                    Console.WriteLine($"{player.NameWithId} has joined the game.");
                    NotifyOtherThatOpponentFound(player);
                    break;

                default:
                    Console.WriteLine($"{player.NameWithId} sent this:\n{data}");
                    break;
            }
        }

        private void NotifyOtherThatOpponentFound(Player player)
        {
            if (players.Count <= 1)
                return;

            var other = OpponentOf(player);
            other.Writer.Write("opponentFound");
            Console.WriteLine($"Sent \"opponentFound\" to {other.NameWithId}");
        }
    }
}
