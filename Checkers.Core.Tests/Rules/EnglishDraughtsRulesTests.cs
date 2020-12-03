using Xunit;
using System;
using Checkers.Core.Board;
using Checkers.Core.Rules;
using Xunit.Abstractions;
using System.Linq;

namespace Checkers.Core.Tests.Rules
{
    public class EnglishDraughtsRulesTests
    {
        private readonly ITestOutputHelper testOutput;

        public EnglishDraughtsRulesTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Fact]
        public void GetMoves_SingleSimpleMoveForward()
        {
            var figure = Figure.CreateSimple(1, 1, Side.Black);
            var board = new SquareBoard(2);
            board.Set(figure);

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var figureMoves = Assert.Contains(figure, moves);
            var move = Assert.Single(figureMoves);
            var step = Assert.Single(move);
            Assert.Equal(MoveStep.Move(Point.At(0, 0)), step);
        }

        [Fact]
        public void GetMoves_SingleSimple_ForwardOnly()
        {
            /* . . . .
             * . . . .
             * . b . . 
             * . . . . */
            var board = new SquareBoard(4);
            board.Set(Figure.CreateSimple(2, 1, Side.Black));

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var figureMoves = moves.Single().Value;
            var expectedTopLeft = new MoveSequence(MoveStep.Move(1, 0));
            var expectedTopRight = new MoveSequence(MoveStep.Move(1, 2));
            Assert.Contains(expectedTopLeft, figureMoves);
            Assert.Contains(expectedTopRight, figureMoves);
            Assert.Equal(2, figureMoves.Length);
        }

        [Fact]
        public void GetMoves_SingleKinge_AllEmptySides()
        {
            /* . . . .
             * . . b .
             * . β . . 
             * b . . . */
            var board = new SquareBoard(4);
            var king = Figure.CreateKing(2, 1, Side.Black);
            board.Set(king);
            board.Set(Figure.CreateSimple(3, 0, Side.Black));
            board.Set(Figure.CreateSimple(1, 2, Side.Black));

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var figureMoves = moves[king];
            var expectedTopLeft = new MoveSequence(MoveStep.Move(1, 0));
            var expectedBottomRight = new MoveSequence(MoveStep.Move(3, 2));
            Assert.Contains(expectedTopLeft, figureMoves);
            Assert.Contains(expectedBottomRight, figureMoves);
            Assert.Equal(2, figureMoves.Length);
        }

        [Fact]
        public void GetMoves_SingleJumpMoveForward()
        {
            var figure = Figure.CreateSimple(3, 3, Side.Black);
            var board = new SquareBoard(4);
            board.Set(figure);
            board.Set(Figure.CreateSimple(2, 2, Side.Red));
            testOutput.WriteLine(board.ToString());

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var figureMoves = Assert.Contains(figure, moves);
            var move = Assert.Single(figureMoves);
            var step = Assert.Single(move);
            Assert.Equal(MoveStep.Jump(Point.At(1, 1)), step);
        }

        [Fact]
        public void GetMoves_SingleJumpAndMakeKingSequence()
        {
            var board = new SquareBoard(3);
            board.Set(Figure.CreateSimple(2, 2, Side.Black));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));
            testOutput.WriteLine(board.ToString());

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var move = moves[Figure.CreateSimple(2, 2, Side.Black)].Single();
            var expectedSequence = new MoveSequence(MoveStep.Jump(0, 0), MoveStep.King());
            Assert.Equal(expectedSequence, move);
        }

        [Fact]
        public void GetMoves_PresetBoard_KingCanJumpBack()
        {
            var board = Board6x6Preset1();
            testOutput.WriteLine(board.ToString());

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var kingMoves = moves[Figure.CreateKing(2, 0, Side.Black)];
            var expectedKingSequence = new MoveSequence(
                MoveStep.Jump(0, 2), 
                MoveStep.Jump(2, 4),
                MoveStep.Jump(0, 6));
            Assert.Equal(expectedKingSequence, kingMoves.Single());
        }

        [Fact]
        public void GetMoves_PresetBoard_SimpleShouldStopWhenPromoted()
        {
            var board = Board6x6Preset1();
            testOutput.WriteLine(board.ToString());

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var simpleMoves = moves[Figure.CreateSimple(4, 6, Side.Black)];
            var jumpUntilPromotedToKing = new MoveSequence(
                MoveStep.Jump(2, 4),
                MoveStep.Jump(0, 2),
                MoveStep.King());
            Assert.Contains(jumpUntilPromotedToKing, simpleMoves);
        }

        [Fact]
        public void GetMoves_PresetBoard_SimpleHasSeveralJumpBranches()
        {
            var board = Board6x6Preset1();
            testOutput.WriteLine(board.ToString());

            var subject = GetSubject();
            var moves = subject.GetMoves(board, Side.Black);

            var simpleMoves = moves[Figure.CreateSimple(4, 6, Side.Black)];
            var expectedSimpleSequence1 = new MoveSequence(
                MoveStep.Jump(2, 4),
                MoveStep.Jump(0, 2),
                MoveStep.King());
            var expectedSimpleSequence2 = new MoveSequence(
                MoveStep.Jump(2, 4),
                MoveStep.Jump(0, 6),
                MoveStep.King());
            Assert.Collection(simpleMoves,
                (x) => Assert.Equal(expectedSimpleSequence1, x),
                (x) => Assert.Equal(expectedSimpleSequence2, x));
        }

        private IRules GetSubject()
        {
            return new EnglishDraughtsRules();
        }

        private static SquareBoard Board6x6Preset1()
        {
            /* 0 1 2 3 4 5 6
             0 . . . . . . .
             1 . r . r . r .
             2 β . . . . . .
             3 . . . . . r .
             4 . . . . . . b
             5 . . . . . . . */
            var board = new SquareBoard(7);
            board.Set(Figure.CreateSimple(1, 1, Side.Red));
            board.Set(Figure.CreateSimple(1, 3, Side.Red));
            board.Set(Figure.CreateSimple(1, 5, Side.Red));
            board.Set(Figure.CreateSimple(3, 5, Side.Red));
            board.Set(Figure.CreateSimple(4, 6, Side.Black));
            board.Set(Figure.CreateKing(2, 0, Side.Black));
            return board;
        }

        //TODO: all general cases are covered, but need to apply few more:
        // any test for Red side
        // Jump the same cell twice is restricted
        // Jump over the boards of the board :)

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
