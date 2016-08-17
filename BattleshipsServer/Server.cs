﻿using System;
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
                ParseTraffic(player, player.ReadTraffic());
        }

        private Player OpponentOf(Player player) => players.Find(a => a != player);

        private static bool IsValid(List<ShipProperties> ships)
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

            if (separatorIndex == -1)
            {
                Console.WriteLine($"{player.NameWithId} sent a packet without a header:\n{traffic}");
                return;
            }

            string header = traffic.Substring(0, separatorIndex);
            string data = traffic.Substring(separatorIndex + 1);
            
            switch (header)
            {
                case Game.NameString:
                    player.Name = data;
                    Console.WriteLine($"{player.NameWithId} has connected to the server.");
                    break;

                case Game.EnterString:
                    var shipPropArray = ShipProperties.DeserializeList(data);

                    if (!IsValid(shipPropArray))
                        throw new Exception("Wrong ship set bruh");

                    player.Ships = shipPropArray.Select(shipProps => new Ship(shipProps)).ToList();
                    
                    player.Status = Player.State.InMatchmaking;
                    Console.WriteLine($"{player.NameWithId} has entered matchmaking.");

                    if (players.Count < 2)
                        break;

                    var opponent = OpponentOf(player);
                    if (opponent.Status != Player.State.InMatchmaking)
                        break;

                    player.Status = Player.State.InGame;
                    opponent.Status = Player.State.InGame;

                    Console.WriteLine($"GAME STARTS: {player.NameWithId} vs {opponent.NameWithId}");

                    matches.Add(new Match(player, opponent));
                    break;

                case Game.LeaveString:
                    if (player.Status == Player.State.InMatchmaking)
                    {
                        Console.WriteLine($"{player.NameWithId} has left matchmaking.");
                        player.Status = Player.State.Available;
                    }
                    else
                        throw new Exception("Player tries to leave matchmaking while not in it");
                    break;

                case Game.ShootString:
                    var split = data.Split('\'');
                    int x = Int32.Parse(split[0]);
                    int y = Int32.Parse(split[1]);

                    var match = matches.Find(m => !m.Over && (m.Player1 == player || m.Player2 == player));
                    match.Shoot(player, x, y);
                    break;

                default:
                    Console.WriteLine($"{player.NameWithId} sent this:\n{traffic}");
                    break;
            }
        }
    }
}
