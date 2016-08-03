using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using BattleshipsCommon;

namespace BattleshipsServer
{
    public class Player
    {
        public readonly TcpClient Client;
        public readonly BinaryReader Reader;
        public readonly BinaryWriter Writer;
        public string Name;
        public readonly int Id;
        public bool InMatchmaking = false;
        public List<Ship> Ships;

        private static int id = 1;

        public string NameWithId => $"{Name} [{Id}]";

        public Player(TcpClient client)
        {
            Id = id++;
            Client = client;
            var stream = client.GetStream();
            Reader = new BinaryReader(stream);
            Writer = new BinaryWriter(stream);
        }

        //public bool Equals(Player player) => Id == player?.Id;

        private void Send(string text)
        {
            Writer.Write(text);
            Console.WriteLine($"Sent \"{text}\" to {NameWithId}");
        } 

        public void NotifyYourTurn() => Send("yourTurn");

        public void NotifyOpponentsTurn() => Send("opponentsTurn");

        public void NotifyYouMissed() => Send("youMissed");

        public void NotifyYouHit() => Send("youHit");

        public void NotifyOpponentMissed(int x, int y) => Send($"opponentMissed:{x}'{y}");

        public void NotifyOpponentHit(int x, int y) => Send($"opponentHit:{x}'{y}");
    }
}
