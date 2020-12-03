using Checkers.Core.Board;
using System;
using Xunit;

namespace Checkers.Core.Tests
{
    public class BoardTests
    {
        [Fact]
        public void Ctor_EnsureBoardIsEmpty()
        {
            var subject = new SquareBoard(8);

            Console.Write(subject.ToString());

            AllCells(pos =>
            {
                Assert.Equal(Side.Empty, subject.Get(pos).Side);
            });
        }

        [Fact]
        public void SetAndGet_BlackFigure()
        {
            var subject = new SquareBoard(8);

            subject.Set(new Figure(new Point(1, 1), Side.Black));

            Assert.Equal(Side.Black, subject.Get(new Point(1, 1)).Side);
        }

        [Fact]
        public void SetAndGet_RedFigure()
        {
            var subject = new SquareBoard(8);

            subject.Set(new Figure(new Point(7, 7), Side.Red));

            Assert.Equal(Side.Red, subject.Get(new Point(7, 7)).Side);
        }

        [Fact]
        public void SetAndGet_MixedFiguresOnEdges_CheckAllWithEmptyCells()
        {
            var subject = new SquareBoard(8);
            var topLeft = new Point(0, 0);
            var topRight = new Point(0, 7);
            var bottomLeft = new Point(7, 0);
            var bottomRight = new Point(7, 7);

            subject.Set(new Figure(topLeft, Side.Red));
            subject.Set(new Figure(topRight, Side.Black));
            subject.Set(new Figure(bottomLeft, Side.Red));
            subject.Set(new Figure(bottomRight, Side.Black));

            Console.Write(subject.ToString());
            AllCells(pos =>
            {
                if (pos == topLeft || pos == bottomLeft)
                    Assert.Equal(Side.Red, subject.Get(pos).Side);
                else if (pos == topRight || pos == bottomRight)
                    Assert.Equal(Side.Black, subject.Get(pos).Side);
                else
                    Assert.Equal(Side.Empty, subject.Get(pos).Side);
            });
        }

        [Fact]
        public void IsEmpty_Empty()
        {
            var subject = new SquareBoard(8);
            var topLeft = new Point(0, 0);
            var bottomRight = new Point(7, 7);

            subject.Set(new Figure(topLeft, Side.Red));
            subject.Set(new Figure(bottomRight, Side.Black));

            AllCells(pos =>
            {
                if (pos != topLeft && pos != bottomRight)
                    Assert.True(subject.IsEmpty(pos));
            });
        }

        [Fact]
        public void IsEmpty_NonEmpty()
        {
            var subject = new SquareBoard(8);
            var topLeft = new Point(0, 0);
            var bottomRight = new Point(7, 7);

            subject.Set(new Figure(topLeft, Side.Red));
            subject.Set(new Figure(bottomRight, Side.Black));

            Assert.False(subject.IsEmpty(topLeft));
            Assert.False(subject.IsEmpty(bottomRight));
        }

        [Fact]
        public void IsEmpty_OutOfBounds_NotEmpty()
        {
            var subject = new SquareBoard(8);

            Assert.False(subject.IsEmpty(new Point(-1, 0)));
            Assert.False(subject.IsEmpty(new Point(0, 8)));
            Assert.False(subject.IsEmpty(new Point(8, 8)));
            Assert.False(subject.IsEmpty(new Point(-1, 7)));
        }

        [Fact]
        public void King_NoKingsForEmptyBoard()
        {
            var subject = new SquareBoard(8);

            AllCells(pos => { Assert.False(subject.Get(pos).IsKing); });
        }

        [Fact]
        public void King_SetAndCheck()
        {
            var subject = new SquareBoard(8);

            var position = new Point(1, 1);
            subject.Set(new Figure(new Point(1, 1), Side.Black, isKing: true));

            Assert.True(subject.Get(position).IsKing);
        }

        [Fact]
        public void King_MoveKing_ShouldResetPreviousPosition()
        {
            var subject = new SquareBoard(8);

            var position = new Point(1, 1);
            subject.Set(new Figure(new Point(1, 1), Side.Black, isKing: true));
            subject.Set(new Figure(position, Side.Empty));

            Assert.False(subject.Get(position).IsKing);
        }

        private void AllCells(Action<Point> Assertion)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Assertion(new Point(i, j));
                }
            }
        }
    }
}
