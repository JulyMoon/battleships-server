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

        private Player Other(Player player) => players.Find(a => !a.Equals(player));

        private void ParseTraffic(Player player, string data)
        {
            int separatorIndex = data.IndexOf(":", StringComparison.Ordinal);

            if (separatorIndex == -1)
                throw new ArgumentException("Invalid data");

            string header = data.Substring(0, separatorIndex);
            data = data.Substring(separatorIndex + 1);

            string message;
            switch (header)
            {
                case "name":
                    player.Name = data;
                    message = $"{player.Name} has joined the game.";
                    NotifyOtherThatOpponentFound(player);
                    break;

                //case "message":
                //    message = $"{player.Name}: {data}";
                //    break;

                default: //throw new ArgumentException("Invalid data");
                    message = $"{player.Name} sent this:\n{data}";
                    break;
            }

            Console.WriteLine(message);
            // todo
        }

        private void NotifyOtherThatOpponentFound(Player player)
        {
            if (players.Count > 1)
                Other(player).Writer.Write("opponentFound");
        }
    }
}
