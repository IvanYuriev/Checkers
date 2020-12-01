using System;

namespace Checkers.Core
{
    public class Move
    {
        public static Move Create(Point from, Point to) => new Move(from, to);

        public Point From { get; private set; }
        public Point To { get; private set; }

        public Move(Point from, Point to)
        {
            if (from == Point.Nop || to == Point.Nop)
                throw new ArgumentException("Only valid positions are allowed!");

            From = from;
            To = to;
        }

        public int Distance => Math.Abs(From.Row - To.Row); //only diagonal moves exist

        public override string ToString()
        {
            return $"{From}->{To}: {Distance}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return false;
            if (obj.GetType() != typeof(Move)) return false;

            var that = (Move)obj;

            return that.From == this.From && that.To == this.To;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To);
        }
    }
}