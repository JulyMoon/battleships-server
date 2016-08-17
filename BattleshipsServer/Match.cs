using System;
using BattleshipsCommon;

namespace BattleshipsServer
{
    public class Match
    {
        public Player Player1 { get; }
        public Player Player2 { get; }
        public bool Turn { get; private set; }

        public bool Over { get; private set; }
        public Player Winner { get; private set; }
        public Player Loser { get; private set; }

        private Player attacker => Turn ? Player1 : Player2;
        private Player defender => Turn ? Player2 : Player1;

        private static readonly Random random = new Random();

        public Match(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;

            Turn = random.Next(2) == 0;

            attacker.SendYourTurn();
            defender.SendOpponentsTurn();
        }

        public void Shoot(Player player, int x, int y)
        {
            if (Over)
                throw new Exception("Yo wtf this game is already over");

            if (player != attacker)
            {
                if (player == defender)
                    throw new Exception("Defender tried to attack. Lol.");

                throw new Exception("WTF? This player isn't from this match");
            }

            if (!Game.WithinBoard(x, y))
                throw new Exception("You shootin' outside of board?");
            
            int index, segment;
            bool hit = Game.GetShotShipSegment(defender.Ships, x, y, out index, out segment);
            if (hit)
            {
                defender.Ships[index].IsAlive[segment] = false;

                if (defender.Ships[index].Dead)
                    attacker.SendYouSank();
                else
                    attacker.SendYouHit();
            }
            else
            {
                attacker.SendYouMissed();
            }

            defender.SendOpponentShot(x, y);

            if (hit)
            {
                Console.WriteLine(defender.Ships[index].Dead
                    ? $"{attacker.NameWithId} just sank {defender.NameWithId}'s ship at ({x}, {y})"
                    : $"{attacker.NameWithId} just hit {defender.NameWithId} at ({x}, {y})");

                if (defender.Ships.TrueForAll(ship => ship.Dead))
                {
                    Over = true;
                    Player1.Status = Player.State.Available;
                    Player2.Status = Player.State.Available;
                    Winner = attacker;
                    Loser = defender;
                    Console.WriteLine($"GAME OVER | WINNER: {attacker.NameWithId} | LOSER: {defender.NameWithId}");
                }
            }
            else
            {
                Console.WriteLine($"{attacker.NameWithId} just missed {defender.NameWithId} at ({x}, {y})");
                Turn = !Turn;
            }
        }
    }
}
