using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public class Direction
    {
        public static Func<Point, int, Point> Nop = new Func<Point, int, Point>((p, steps) 
            => throw new NotSupportedException("NOP direction should not be used!"));

        public static Func<Point, int, Point> UpperLeft = new Func<Point, int, Point>((p, steps) => new Point(p.Row - steps, p.Col - steps));
        public static Func<Point, int, Point> UpperRight = new Func<Point, int, Point>((p, steps) => new Point(p.Row - steps, p.Col + steps));
        public static Func<Point, int, Point> BottomLeft = new Func<Point, int, Point>((p, steps) => new Point(p.Row + steps, p.Col - steps));
        public static Func<Point, int, Point> BottomRight = new Func<Point, int, Point>((p, steps) => new Point(p.Row + steps, p.Col + steps));
    }

    public struct Figure
    {
        public static Figure Nop = new Figure(Point.Nop, Side.Nop);

        internal readonly Func<int, Point>[] Directions;

        public Point Point { get; }
        public Side Side { get; }
        public bool IsKing { get; }

        private static Func<T2, TResult> ApplyPartial<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 arg1)
        {
            return (b) => function(arg1, b);
        }

        public static Figure CreateSimple(int row, int col, Side side) => new Figure(new Point(row, col), side);
        public static Figure CreateKing(int row, int col, Side side) => new Figure(new Point(row, col), side, isKing: true);

        public Figure(Point point, Side side, bool isKing = false)
        {
            Point = point;
            Side = side;
            IsKing = isKing;
            Directions = new[] { ApplyPartial(Direction.Nop, point) };

            if (IsKing)
            {
                Directions = new[]
                {
                    ApplyPartial(Direction.BottomLeft, point),
                    ApplyPartial(Direction.BottomRight, point),
                    ApplyPartial(Direction.UpperLeft, point),
                    ApplyPartial(Direction.UpperRight, point)
                };
            }
            else if (Side == Side.Black)
            {
                Directions = new[] 
                {
                    ApplyPartial(Direction.UpperLeft, point),
                    ApplyPartial(Direction.UpperRight, point)
                };
            }
            else if (Side == Side.Red)
            {
                Directions = new[] 
                {
                    ApplyPartial(Direction.BottomLeft, point),
                    ApplyPartial(Direction.BottomRight, point)
                };
            }
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
