using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public struct Board
    {
        public const int SIZE = 8;
        //bit field for board of figures, each bit encode the state of cell on the board:
        //  1 - means the cell has BLACK figure
        //  0 - means the cell is NOT have black figure (stil could contain RED figure)
        private ulong _blackFigures;
        // the same as _blackFigures but for RED figures
        private ulong _redFigures;
        //bit field of kings on the board: 1 - is a King, 0 - is not
        private ulong _kingsMask; //TODO: implement kings!

        //PERF: white cells are not used, can we utilize it somehow?

        public Figure Get(Point p)
        {
            EnsureBounds(p);

            var bitPosition = SIZE * p.Y + p.X;
            if (IsBlack(bitPosition)) return new Figure(p, Side.Black, IsKing(bitPosition));
            if (IsRed(bitPosition)) return new Figure(p, Side.Red, IsKing(bitPosition));

            return new Figure(p, Side.None);
        }

        public bool IsEmpty(Point p)
        {
            //out of bound cells are treated as NON empty
            if (p.X < 0 || p.X >= SIZE) return false;
            if (p.Y < 0 || p.Y >= SIZE) return false;

            var bitPosition = SIZE * p.Y + p.X;
            return !IsBlack(bitPosition) && !IsRed(bitPosition);
        }

        public void Set(Figure cell)
        {
            EnsureBounds(cell.Point);

            var bitPosition = SIZE * cell.Point.Y + cell.Point.X;
            SetKingsMaskBit(cell, bitPosition);
            if (cell.Side == Side.Red)
            {
                _blackFigures &= ~((ulong)1 << bitPosition);
                _redFigures |= (ulong)1 << bitPosition;
            }
            else if (cell.Side == Side.Black)
            {
                _blackFigures |= (ulong)1 << bitPosition;
                _redFigures &= ~((ulong)1 << bitPosition);
            }
            else //EMPTY
            {
                _blackFigures &= ~((ulong)1 << bitPosition);
                _redFigures &= ~((ulong)1 << bitPosition);
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            for (byte i = 0; i < SIZE; i++)
            {
                for (byte j = 0; j < SIZE; j++)
                {
                    var p = new Point(i, j);
                    var cell = Get(p);
                    switch(cell.Side)
                    {
                        case Side.None:
                            result.Append('o');
                            break;
                        case Side.Black:
                            result.Append('b');
                            break;
                        case Side.Red:
                            result.Append('r');
                            break;
                    }
                    result.Append(' ');
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        private static void EnsureBounds(Point p)
        {
            Contract.Ensures(p.X < 8);
            Contract.Ensures(p.Y < 8);
            Contract.Ensures(p.X >= 0);
            Contract.Ensures(p.Y >= 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsKing(int bitPosition)
        {
            return ((_kingsMask >> bitPosition) & 1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBlack(int bitPosition)
        {
            return ((_blackFigures >> bitPosition) & 1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRed(int bitPosition)
        {
            return ((_redFigures >> bitPosition) & 1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetKingsMaskBit(Figure cell, int bitPosition)
        {
            if (cell.IsKing)
                _kingsMask |= (ulong)1 << bitPosition;
            else
                _kingsMask &= ~((ulong)1 << bitPosition);
        }
    }
}
