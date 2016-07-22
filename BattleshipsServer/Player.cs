using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsServer
{
    public class Player
    {
        public readonly TcpClient Client;
        public readonly BinaryReader Reader;
        public readonly BinaryWriter Writer;
        public string Name;
        public readonly int Id;
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

        public bool Equals(Player player) => Client == player?.Client;
    }
}
