using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Board
{
    public struct SquareBoard
    {
        //bit field for board of figures, each bit encode the state of cell on the board:
        //  1 - means the cell has BLACK figure
        //  0 - means the cell is NOT have black figure (stil could contain RED figure)
        private ulong _blackFigures;
        // the same as _blackFigures but for RED figures
        private ulong _redFigures;
        //bit field of kings on the board: 1 - is a King, 0 - is not
        private ulong _kingsMask; //TODO: implement kings!

        //PERF: white cells are not used, uint is enough for each Side!

        public int Size { get; private set; }

        public SquareBoard(int size) : this()
        {
            Size = size;
        }

        public Figure Get(Point p)
        {
            if (p == Point.Nop || p.Col >= Size || p.Row >= Size) return Figure.Nop;

            var bitPosition = Size * p.Col + p.Row;
            if (IsBlack(bitPosition)) return new Figure(p, Side.Black, IsKing(bitPosition));
            if (IsRed(bitPosition)) return new Figure(p, Side.Red, IsKing(bitPosition));

            return new Figure(p, Side.Empty);
        }

        public bool IsEmpty(Point p)
        {
            //out of bound cells are treated as NON empty
            if (p.Row < 0 || p.Row >= Size) return false;
            if (p.Col < 0 || p.Col >= Size) return false;

            var bitPosition = Size * p.Col + p.Row;
            return !IsBlack(bitPosition) && !IsRed(bitPosition);
        }

        internal bool NoFigures(Side playerSide)
        {
            //TODO: NOT only Black and Red sides exist, FIXIT!
            return playerSide == Side.Black ? _blackFigures == 0 : _redFigures == 0; 
        }

        public void Set(Figure cell)
        {
            EnsureBounds(cell.Point);

            var bitPosition = Size * cell.Point.Col + cell.Point.Row;
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

        public void Clear(Point p)
        {
            EnsureBounds(p);

            var bitPosition = Size * p.Col + p.Row;
            _blackFigures &= ~((ulong)1 << bitPosition);
            _redFigures &= ~((ulong)1 << bitPosition);
            _kingsMask &= ~((ulong)1 << bitPosition);
        }

        public void SetKing(Point p)
        {
            EnsureBounds(p);

            var bitPosition = Size * p.Col + p.Row;
            _kingsMask |= (ulong)1 << bitPosition;
        }

        public void ClearKing(Point p)
        {
            EnsureBounds(p);

            var bitPosition = Size * p.Col + p.Row;
            _kingsMask &= ~((ulong)1 << bitPosition);
        }

        public Figure[] GetAll(Side side)
        {
            if (side <= Side.Empty) throw new ArgumentException($"Expected valid side, but got {side}");

            var result = new List<Figure>(); //TODO: could be improved - fast path!
            var figures = side == Side.Black ? _blackFigures : _redFigures;
            for (byte i = 0; i < Size; i++)
            {
                for (byte j = 0; j < Size; j++)
                {
                    var bitPosition = Size * j + i;
                    if (((figures >> bitPosition) & 1) == 1)
                    {
                        result.Add(new Figure(Point.At(i, j), side, IsKing(bitPosition)));
                    }
                }
            }
            return result.ToArray();
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            for (byte i = 0; i < Size; i++)
            {
                for (byte j = 0; j < Size; j++)
                {
                    var p = new Point(i, j);
                    var cell = Get(p);
                    switch(cell.Side)
                    {
                        case Side.Empty:
                            result.Append('.');
                            break;
                        case Side.Black:
                            if (cell.IsKing) result.Append('B'); else result.Append('b');
                            break;
                        case Side.Red:
                            if (cell.IsKing) result.Append('R'); else result.Append('r');
                            break;
                    }
                    result.Append(' ');
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBounds(Point p)
        {
            Contract.Ensures(p.Row < Size);
            Contract.Ensures(p.Col < Size);
            Contract.Ensures(p.Row >= 0);
            Contract.Ensures(p.Col >= 0);
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
