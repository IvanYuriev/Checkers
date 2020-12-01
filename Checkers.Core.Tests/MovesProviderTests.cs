using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Tests
{
    [TestClass]
    public class MovesProviderTests
    {
        [TestMethod]
        public void GetMoves2x2_RedInTopLeftCorner_HasSingleMove()
        {
            var board = new SquareBoard(2);
            board.Set(Figure.CreateSimple(0, 0, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);

            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(Point.At(0, 0), moves[0].From);
            Assert.AreEqual(Point.At(1, 1), moves[0].To);
        }

        [TestMethod]
        public void GetMoves2x2_TwoAdjacentReds_NoMoves()
        {
            var board = new SquareBoard(2);
            board.Set(Figure.CreateSimple(0, 0, Side.Red));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);

            Assert.AreEqual(0, moves.Count);
        }

        [TestMethod]
        public void GetMoves3x3_TwoAdjacentReds_SecondOneCanMoveOnly()
        {
            var board = new SquareBoard(3);
            board.Set(Figure.CreateSimple(0, 0, Side.Red));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);

            Assert.AreEqual(2, moves.Count);
            Assert.AreEqual(Point.At(1, 1), moves[0].From);
            Assert.AreEqual(Point.At(2, 0), moves[0].To);
            Assert.AreEqual(Point.At(1, 1), moves[1].From);
            Assert.AreEqual(Point.At(2, 2), moves[1].To);
        }

        [TestMethod]
        public void GetMoves3x3_TwoAdjacentReds_SingleJumpOnly()
        {
            var board = new SquareBoard(3);
            board.Set(Figure.CreateSimple(0, 0, Side.Red));
            board.Set(Figure.CreateSimple(1, 1, Side.Black));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);

            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(Point.At(0, 0), moves[0].From);
            Assert.AreEqual(Point.At(2, 2), moves[0].To);
        }

        [TestMethod]
        public void GetMoves3x3_SingleRedInTheMiddle_ForwardMovesOnly()
        {
            var board = new SquareBoard(3);
            board.Set(Figure.CreateSimple(1, 1, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);

            Assert.AreEqual(Point.At(2, 0), moves[0].To);
            Assert.AreEqual(Point.At(2, 2), moves[1].To);
        }

        [TestMethod]
        public void GetMoves3x3_KingRedInTheMiddle_AllSides()
        {
            var board = new SquareBoard(3);
            board.Set(Figure.CreateKing(1, 1, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);

            Assert.AreEqual(Point.At(2, 0), moves[0].To);
            Assert.AreEqual(Point.At(2, 2), moves[1].To);
            Assert.AreEqual(Point.At(0, 0), moves[2].To);
            Assert.AreEqual(Point.At(0, 2), moves[3].To);
        }

        [TestMethod]
        public void GetMoves4x4_BlackAndReds_NoMoves()
        {
            var board = new SquareBoard(4);
            Fill(ref board); //fills in a proper checkers order
            Console.Write(board.ToString());

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Red);
            Assert.AreEqual(0, moves.Count);
        }

        [TestMethod]
        public void GetMoves4x4_BlackAndReds_SingleMoveForBlacks()
        {
            var board = new SquareBoard(4);
            Fill(ref board);
            board.Clear(Point.At(1, 3)); //REMOVE one red figure

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Black);

            Assert.AreEqual(Point.At(2, 2), moves[0].From);
            Assert.AreEqual(Point.At(1, 3), moves[0].To);
            Assert.AreEqual(1, moves.Count);
        }

        [TestMethod]
        public void GetMoves4x4_BlackAndReds_BlackCanMakeAJump()
        {
            var board = new SquareBoard(4);
            Fill(ref board);
            board.Clear(Point.At(0, 0)); //REMOVE one red figure

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Black);

            Assert.AreEqual(Point.At(2, 2), moves[0].From);
            Assert.AreEqual(Point.At(0, 0), moves[0].To);
            Assert.AreEqual(1, moves.Count);
        }

        [TestMethod]
        public void GetMoves4x4_BlackAndReds_SimpleBlackCannotJumpBack()
        {
            var board = new SquareBoard(4);
            board.Set(Figure.CreateSimple(0, 0, Side.Black));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Black);

            Assert.AreEqual(0, moves.Count);
        }

        [TestMethod]
        public void GetMoves4x4_BlackAndReds_KingBlackCanJumpBack()
        {
            var board = new SquareBoard(4);
            board.Set(Figure.CreateKing(0, 0, Side.Black));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));

            var subject = new MovesProvider(board);

            var moves = subject.GetMoves(Side.Black);

            Assert.AreEqual(Point.At(0, 0), moves[0].From);
            Assert.AreEqual(Point.At(2, 2), moves[0].To);
            Assert.AreEqual(1, moves.Count);
        }

        [TestMethod]
        public void GetMoves4x4_Preset_AllKindOfMovesArePossible()
        {
            /* o o o r
             * b o β o
             * o o o b
             * b o b o 
             */
            var board = new SquareBoard(4);
            board.Set(Figure.CreateSimple(0, 3, Side.Red));
            board.Set(Figure.CreateSimple(1, 0, Side.Black));
            board.Set(Figure.CreateKing(1, 2, Side.Black));
            board.Set(Figure.CreateSimple(2, 3, Side.Black));
            board.Set(Figure.CreateSimple(3, 0, Side.Black));
            board.Set(Figure.CreateSimple(3, 2, Side.Black));

            var subject = new MovesProvider(board);

            var blackMoves = subject.GetMoves(Side.Black);
            var redMoves = subject.GetMoves(Side.Red);

            Assert.AreEqual(Move.Create(Point.At(1, 0), Point.At(0, 1)), blackMoves[0]);
            Assert.AreEqual(Move.Create(Point.At(1, 2), Point.At(2, 1)), blackMoves[1]);
            Assert.AreEqual(Move.Create(Point.At(1, 2), Point.At(0, 1)), blackMoves[2]);
            Assert.AreEqual(Move.Create(Point.At(3, 0), Point.At(2, 1)), blackMoves[3]);
            Assert.AreEqual(Move.Create(Point.At(3, 2), Point.At(2, 1)), blackMoves[4]);

            Assert.AreEqual(Move.Create(Point.At(0, 3), Point.At(2, 1)), redMoves[0]);

            Assert.AreEqual(6, blackMoves.Count + redMoves.Count);
        }

        private static void Fill(ref SquareBoard board)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((i + j) % 2 == 1) continue;
                    var side = i < 2 ? Side.Red : Side.Black;
                    board.Set(Figure.CreateSimple(i, j, side));
                }
            }
        }
    }
}
