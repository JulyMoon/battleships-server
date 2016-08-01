﻿using System;
using System.IO;
using System.Net.Sockets;

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
        public string Ships;

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
    }
}
