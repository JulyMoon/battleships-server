using System;

namespace BattleshipsServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Battleships Server";
            new Server().Start();
            //Console.Title += $" | IP: {Game.ServerHostname}"; //{Game.ServerIP}";

            while (true)
                Console.ReadLine();
        }
    }
}
