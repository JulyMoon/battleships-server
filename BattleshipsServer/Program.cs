using System;
using System.Net;

namespace BattleshipsServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Battleships Server";
            new Server().Start();
            Console.Title += $" | IP: {new WebClient().DownloadString("http://icanhazip.com")}";

            while (true)
                Console.ReadLine();
        }
    }
}
