using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BattleshipsCommon;

namespace BattleshipsServer
{
    public class Player
    {
        public enum State { Available, InMatchmaking, InGame }

        private readonly TcpClient client;
        private readonly BinaryReader reader;
        private readonly BinaryWriter writer;
        public string Name;
        public readonly int Id;
        public State Status = State.Available;
        public List<Ship> Ships;
        public IPAddress IP { get; private set; }

        //public bool ClientConnected => client.Connected;

        private static int id = 1;

        public string NameWithId => $"{Name} [{Id}]";

        public Player(TcpClient client)
        {
            Id = id++;
            this.client = client;
            var stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
            IP = (client.Client.LocalEndPoint as IPEndPoint)?.Address;
        }

        private void Send(string text)
        {
            writer.Write(text);
            //Console.WriteLine($"Sent \"{text}\" to {NameWithId}");
        }

        public void CloseConnection() => client.Close();

        public string ReadTraffic() => reader.ReadString();

        public void SendYourTurn() => Send(Game.YourTurnString);

        public void SendOpponentsTurn() => Send(Game.OpponentsTurnString);

        public void SendYouMissed() => Send(Game.YouMissedString);

        public void SendYouHit() => Send(Game.YouHitString);

        public void SendYouSank() => Send(Game.YouSankString);

        public void SendOpponentShot(int x, int y) => Send($"{Game.OpponentShotString}:{x}'{y}");
    }
}
