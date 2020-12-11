using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Board
{
    public struct Figure
    {
        public static Figure Nop = new Figure(Point.Nop, Side.Nop);

        public Point Point { get; }
        public Side Side { get; }
        public bool IsKing { get; }

        public static Figure CreateSimple(int row, int col, Side side) => new Figure(new Point(row, col), side);
        public static Figure CreateKing(int row, int col, Side side) => new Figure(new Point(row, col), side, isKing: true);

        public Figure(Point point, Side side, bool isKing = false)
        {
            Point = point;
            Side = side;
            IsKing = isKing;
        }

        public override string ToString()
        {
            var sideChar = Side.ToString()[0];
            if (IsKing) sideChar = Char.ToUpper(sideChar);
            return $"{sideChar}{Point}";
        }

        public override int GetHashCode() => (Point, Side).GetHashCode();

        public override bool Equals(object obj) => obj is Figure m && Equals(m);

        public bool Equals(Figure other) => Side == other.Side && Point == other.Point && IsKing == other.IsKing;

        public static bool operator ==(Figure left, Figure right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Figure left, Figure right)
        {
            return !(left == right);
        }
    }
}
