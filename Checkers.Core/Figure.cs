using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public struct Figure
    {
        public Point Point { get; }
        public Side Side { get; }
        public bool IsKing { get; }

        public Figure(Point point, Side side, bool isKing = false)
        {
            Point = point;
            Side = side;
            IsKing = isKing;
        }

        public override string ToString()
        {
            var isKingMark = IsKing ? "*" : "";
            return $"{Point}:{Side}{isKingMark}";
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
        //TODO: override others
    }
}
