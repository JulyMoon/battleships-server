﻿using System.Collections.ObjectModel;
using System.Linq;

namespace BattleshipsServer
{
    public class Ship : ShipProperties
    {
        private readonly bool[] isAlive;

        public ReadOnlyCollection<bool> IsAlive => isAlive.ToList().AsReadOnly();
        public bool Dead => isAlive.All(cell => !cell);

        public Ship(int size, bool isVertical, int x, int y) : base(size, isVertical, x, y)
        {
            isAlive = Enumerable.Repeat(true, size).ToArray();
        }

        public Ship(ShipProperties shipProps) : this(shipProps.Size, shipProps.IsVertical, shipProps.X, shipProps.Y) { }
    }
}
