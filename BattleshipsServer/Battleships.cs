using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BattleshipsServer
{
    public static class Battleships
    {
        public const int BoardWidth = 10;
        public const int BoardHeight = BoardWidth;
        private static readonly int[] shipSet = {4, 3, 3, 2, 2, 2, 1, 1, 1, 1};
        private static readonly int[,] neighborsAndItselfPoints = { {-1, -1}, {0, -1}, {1, -1}, {-1, 0}, {0, 0}, {1, 0}, {-1, 1}, {0, 1}, {1, 1} };

        public static ReadOnlyCollection<int> ShipSet => Array.AsReadOnly(shipSet);

        public static void GetShipDimensions(bool vertical, int size, out int shipW, out int shipH)
        {
            if (vertical)
            {
                shipW = 1;
                shipH = size;
            }
            else
            {
                shipW = size;
                shipH = 1;
            }
        }

        public static bool WithinBoard(ShipProperties props)
        {
            int width, height;
            GetShipDimensions(props.IsVertical, props.Size, out width, out height);

            return WithinBoard(props.X, props.Y) && props.X + width - 1 < BoardWidth && props.Y + height - 1 < BoardHeight;
        }

        public static bool WithinBoard(int x, int y) => x >= 0 && x < BoardWidth && y >= 0 && y < BoardHeight;

        public static bool Overlaps(IEnumerable<ShipProperties> ships, ShipProperties other)
        {
            var grid = new bool[BoardWidth, BoardHeight];
            foreach (var ship in ships)
            {
                for (int i = 0; i < ship.Size; i++)
                {
                    if (ship.IsVertical)
                        grid[ship.X, ship.Y + i] = true;
                    else
                        grid[ship.X + i, ship.Y] = true;
                }
            }

            for (int i = 0; i < other.Size; i++)
            {
                int x, y;
                if (other.IsVertical)
                {
                    x = other.X;
                    y = other.Y + i;
                }
                else
                {
                    x = other.X + i;
                    y = other.Y;
                }

                for (int j = 0; j < 9; j++)
                {
                    int xx = x + neighborsAndItselfPoints[j, 0];
                    int yy = y + neighborsAndItselfPoints[j, 1];

                    if (WithinBoard(xx, yy) && grid[xx, yy])
                        return true;
                }
            }

            return false;
        }
    }
}
