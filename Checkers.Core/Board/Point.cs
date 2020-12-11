using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Board
{
    public struct Point
    {
        public static Point Nop = new Point(-1);
        public static Point At(int row, int col) => new Point(row, col);

        private Point(int emptyValue)
        {
            Row = emptyValue;
            Col = emptyValue;
        }

        public Point(int row, int col)
        {
            if (row < 0 || col < 0)
            {
                this = Nop;
                return;
            }

            Row = row;
            Col = col;
        }

        public int Row { get; }
        public int Col { get; }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return false;
            if (obj.GetType() != typeof(Point)) return false;

            var that = (Point)obj;

            return that.Row == this.Row && that.Col == this.Col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Row == right.Row && left.Col == right.Col;
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }
    }
}
