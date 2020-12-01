using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Checkers.Core.Tests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void Ctor_EnsureBoardIsEmpty()
        {
            var subject = new SquareBoard(8);

            Console.Write(subject.ToString());

            AllCells(pos =>
            {
                Assert.AreEqual(Side.Empty, subject.Get(pos).Side);
            });
        }

        [TestMethod]
        public void SetAndGet_BlackFigure()
        {
            var subject = new SquareBoard(8);

            subject.Set(new Figure(new Point(1, 1), Side.Black));

            Assert.AreEqual(Side.Black, subject.Get(new Point(1, 1)).Side);
        }

        [TestMethod]
        public void SetAndGet_RedFigure()
        {
            var subject = new SquareBoard(8);

            subject.Set(new Figure(new Point(7, 7), Side.Red));

            Assert.AreEqual(Side.Red, subject.Get(new Point(7, 7)).Side);
        }

        [TestMethod]
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
                    Assert.AreEqual(Side.Red, subject.Get(pos).Side);
                else if (pos == topRight || pos == bottomRight)
                    Assert.AreEqual(Side.Black, subject.Get(pos).Side);
                else
                    Assert.AreEqual(Side.Empty, subject.Get(pos).Side);
            });
        }

        [TestMethod]
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
                    Assert.IsTrue(subject.IsEmpty(pos));
            });
        }

        [TestMethod]
        public void IsEmpty_NonEmpty()
        {
            var subject = new SquareBoard(8);
            var topLeft = new Point(0, 0);
            var bottomRight = new Point(7, 7);

            subject.Set(new Figure(topLeft, Side.Red));
            subject.Set(new Figure(bottomRight, Side.Black));

            Assert.IsFalse(subject.IsEmpty(topLeft));
            Assert.IsFalse(subject.IsEmpty(bottomRight));
        }

        [TestMethod]
        public void IsEmpty_OutOfBounds_NotEmpty()
        {
            var subject = new SquareBoard(8);

            Assert.IsFalse(subject.IsEmpty(new Point(-1, 0)));
            Assert.IsFalse(subject.IsEmpty(new Point(0, 8)));
            Assert.IsFalse(subject.IsEmpty(new Point(8, 8)));
            Assert.IsFalse(subject.IsEmpty(new Point(-1, 7)));
        }

        [TestMethod]
        public void King_NoKingsForEmptyBoard()
        {
            var subject = new SquareBoard(8);

            AllCells(pos => { Assert.IsFalse(subject.Get(pos).IsKing); });
        }

        [TestMethod]
        public void King_SetAndCheck()
        {
            var subject = new SquareBoard(8);

            var position = new Point(1, 1);
            subject.Set(new Figure(new Point(1, 1), Side.Black, isKing: true));

            Assert.IsTrue(subject.Get(position).IsKing);
        }

        [TestMethod]
        public void King_MoveKing_ShouldResetPreviousPosition()
        {
            var subject = new SquareBoard(8);

            var position = new Point(1, 1);
            subject.Set(new Figure(new Point(1, 1), Side.Black, isKing: true));
            subject.Set(new Figure(position, Side.Empty));

            Assert.IsFalse(subject.Get(position).IsKing);
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
