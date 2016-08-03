using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BattleshipsCommon;

namespace BattleshipsServer
{
    public class Server
    {
        public const int Port = 7070;

        private readonly List<Player> players = new List<Player>();
        private readonly List<Match> matches = new List<Match>();

        public async void Start()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("Waiting for players...");
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
                    var shipPropArray = ShipProperties.DeserializeList(data);

                    //todo: add overlap/bounds check here

                    player.Ships = shipPropArray.Select(shipProps => new Ship(shipProps)).ToList();
                    
                    player.InMatchmaking = true;
                    Console.WriteLine($"{player.NameWithId} has entered matchmaking.");

                    if (players.Count < 2)
                        break;

                    var opponent = OpponentOf(player);
                    if (!opponent.InMatchmaking)
                        break;

                    player.InMatchmaking = false;
                    opponent.InMatchmaking = false;

                    Console.WriteLine($"GAME STARTS: {player.NameWithId} vs {opponent.NameWithId}");

                    matches.Add(new Match(player, opponent));
                    break;

                case "leave":
                    Console.WriteLine($"{player.NameWithId} has left matchmaking.");
                    player.InMatchmaking = false;
                    break;

                case "shoot":
                    var split = data.Split('\'');
                    int x = Int32.Parse(split[0]);
                    int y = Int32.Parse(split[1]);

                    var match = matches.Find(m => m.Player1 == player || m.Player2 == player);
                    match.Shoot(player, x, y);
                    break;

                default:
                    Console.WriteLine($"{player.NameWithId} sent this:\n{traffic}");
                    break;
            }
        }
    }
}
