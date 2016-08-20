using System;
using System.Collections.Generic;
using System.IO;
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

                Console.WriteLine($"Connection from {client.Client.LocalEndPoint} accepted");
                var player = new Player(client);
                players.Add(player);
                new Thread(() => HandlePlayer(player)).Start();
            }
        }

        private void HandlePlayer(Player player)
        {
            while (true)
            {
                try
                {
                    ParseTraffic(player, player.ReadTraffic());
                }
                catch (IOException)
                {
                    break;
                }
            }

            // todo: tell others that this player dc'd
            player.CloseConnection();
            players.Remove(player);

            Console.WriteLine($"{player.NameWithId} has disconnected from the server");
        }

        private static bool AreValid(List<ShipProperties> ships)
        {
            if (!ships.Select(ship => ship.Size).OrderBy(size => size).SequenceEqual(Game.ShipSet.OrderBy(size => size)))
                return false;

            if (ships.Any(ship => !Game.WithinBoard(ship)))
                return false;

            var correctShips = new List<ShipProperties>();
            foreach (var ship in ships)
            {
                if (Game.Overlaps(correctShips, ship))
                    return false;

                correctShips.Add(ship);
            }

            return true;
        }

        private void ParseTraffic(Player player, string traffic)
        {
            int separatorIndex = traffic.IndexOf(":", StringComparison.Ordinal);

            string header, data;
            if (separatorIndex == -1)
            {
                header = traffic;
                data = null;
            }
            else
            {
                header = traffic.Substring(0, separatorIndex);
                data = traffic.Substring(separatorIndex + 1);
            }
            
            switch (header)
            {
                case Game.NameString: PlayerSetsName(player, data); break;
                case Game.EnterString: PlayerEntersMatchmaking(player, data); break;
                case Game.LeaveString: PlayerLeavesMatchmaking(player); break;
                case Game.ShootString: PlayerShoots(player, data); break;
                default: Console.WriteLine($"{player.NameWithId} sent this:\n{traffic}"); break;
            }
        }

        private static void PlayerSetsName(Player player, string name)
        {
            player.Name = name;
            Console.WriteLine($"{player.NameWithId} has connected to the server");
        }

        private void PlayerEntersMatchmaking(Player player, string rawShips)
        {
            var shipPropArray = ShipProperties.DeserializeList(rawShips);

            if (!AreValid(shipPropArray))
                throw new Exception("Wrong ship set bruh");

            player.Ships = shipPropArray.Select(shipProps => new Ship(shipProps)).ToList();

            player.Status = Player.State.InMatchmaking;
            Console.WriteLine($"{player.NameWithId} has entered matchmaking");

            var opponentCandidates = players.Where(plr => plr.Status == Player.State.InMatchmaking && plr != player).ToList();

            if (opponentCandidates.Count == 0)
                return;

            var opponent = opponentCandidates[0];

            player.Status = Player.State.InGame;
            opponent.Status = Player.State.InGame;

            Console.WriteLine($"GAME STARTS: {player.NameWithId} vs {opponent.NameWithId}");

            matches.Add(new Match(player, opponent));
        }

        private void PlayerLeavesMatchmaking(Player player)
        {
            if (player.Status == Player.State.InMatchmaking)
            {
                Console.WriteLine($"{player.NameWithId} has left matchmaking");
                player.Status = Player.State.Available;
            }
            else
                throw new Exception("Player tries to leave matchmaking while not in it");
        }

        private void PlayerShoots(Player player, string rawCoords)
        {
            var split = rawCoords.Split('\'');
            int x = Int32.Parse(split[0]);
            int y = Int32.Parse(split[1]);

            var match = matches.Find(m => !m.Over && (m.Player1 == player || m.Player2 == player));
            match.Shoot(player, x, y);
        }
    }
}
