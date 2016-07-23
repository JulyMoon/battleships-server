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
                    Console.WriteLine($"{player.NameWithId} has connected to the server.");
                    break;

                case "enter":
                    Console.WriteLine($"{player.NameWithId} has entered matchmaking.");
                    player.InMatchmaking = true;
                    player.Ships = data;

                    if (players.Count < 2)
                        break;

                    var opponent = OpponentOf(player);
                    if (!opponent.InMatchmaking)
                        break;

                    player.InMatchmaking = false;
                    opponent.InMatchmaking = false;

                    Console.WriteLine($"GAME STARTS: {player.NameWithId} vs {opponent.NameWithId}");

                    NotifyThatOpponentFound(player);
                    NotifyThatOpponentFound(opponent);
                    break;

                case "leave":
                    Console.WriteLine($"{player.NameWithId} has left matchmaking.");
                    player.InMatchmaking = false;
                    break;

                default:
                    Console.WriteLine($"{player.NameWithId} sent this:\n{traffic}");
                    break;
            }
        }

        private static void NotifyThatOpponentFound(Player player)
        {
            const string opponentFound = "opponentFound";
            player.Writer.Write(opponentFound);
            Console.WriteLine($"Sent \"{opponentFound}\" to {player.NameWithId}");
        }
    }
}
