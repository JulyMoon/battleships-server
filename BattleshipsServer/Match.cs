using System;
using BattleshipsCommon;

namespace BattleshipsServer
{
    public class Match
    {
        public Player Player1 { get; }
        public Player Player2 { get; }
        public bool Turn { get; private set; }

        private Player Attacker => Turn ? Player1 : Player2;
        private Player Defender => Turn ? Player2 : Player1;

        private static readonly Random random = new Random();

        public Match(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;

            Turn = random.Next(2) == 0;

            Attacker.NotifyYourTurn();
            Defender.NotifyOpponentsTurn();
        }

        public void Shoot(Player player, int x, int y)
        {
            if (player != Attacker)
            {
                if (player == Defender)
                    throw new Exception("Defender tried to attack. Lol.");

                throw new Exception("WTF? This player isn't from this match");
            }

            if (!Game.WithinBoard(x, y))
                throw new Exception("You shootin' outside of board?");
            
            int index, segment;
            bool hit = Game.GetShotShipSegment(Defender.Ships, x, y, out index, out segment);
            if (hit)
            {
                Defender.Ships[index].IsAlive[segment] = false;

                if (Defender.Ships[index].Dead)
                    Attacker.NotifyYouSank();
                else
                    Attacker.NotifyYouHit();
            }
            else
            {
                Attacker.NotifyYouMissed();
            }

            Defender.NotifyOpponentShot(x, y);

            if (hit)
            {
                Console.WriteLine(Defender.Ships[index].Dead
                    ? $"{Attacker.NameWithId} just sank {Defender.NameWithId}'s ship at ({x}, {y})"
                    : $"{Attacker.NameWithId} just hit {Defender.NameWithId} at ({x}, {y})");

                if (Defender.Ships.TrueForAll(ship => ship.Dead))
                    Console.WriteLine($"GAME OVER | WINNER: {Attacker.NameWithId} | LOSER: {Defender.NameWithId}");
            }
            else
            {
                Console.WriteLine($"{Attacker.NameWithId} just missed {Defender.NameWithId} at ({x}, {y})");
                Turn = !Turn;
            }
        }
    }
}
