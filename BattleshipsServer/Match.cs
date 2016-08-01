﻿using System;

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

            // todo: add overlap/bounds check here

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

            if (!Battleships.WithinBoard(x, y))
            {
                throw new Exception("You shootin' outside of board?");
            }
        }
    }
}