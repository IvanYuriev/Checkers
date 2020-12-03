using System;

namespace Checkers.Core.Board
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
}
