using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using BattleshipsCommon;

namespace BattleshipsServer
{
    public class Player
    {
        private readonly TcpClient client;
        private readonly BinaryReader reader;
        private readonly BinaryWriter writer;
        public string Name;
        public readonly int Id;
        public bool InMatchmaking = false;
        public List<Ship> Ships;

        private static int id = 1;

        public string NameWithId => $"{Name} [{Id}]";

        public Player(TcpClient client)
        {
            Id = id++;
            this.client = client;
            var stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }

        private void Send(string text)
        {
            writer.Write(text);
            //Console.WriteLine($"Sent \"{text}\" to {NameWithId}");
        }

        public string ReadTraffic() => reader.ReadString();

        public void NotifyYourTurn() => Send("yourTurn");

        public void NotifyOpponentsTurn() => Send("opponentsTurn");

        public void NotifyYouMissed() => Send("youMissed");

        public void NotifyYouHit() => Send("youHit");

        public void NotifyYouSank() => Send("youSank");

        public void NotifyOpponentShot(int x, int y) => Send($"opponentShot:{x}'{y}");
    }
}
