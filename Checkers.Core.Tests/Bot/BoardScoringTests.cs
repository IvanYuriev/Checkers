using Checkers.Core.Board;
using Checkers.Core.Bot;
using Xunit;
using Xunit.Abstractions;

namespace Checkers.Core.Tests.Bot
{
    public class BoardScoringTests
    {
        private readonly ITestOutputHelper _testOutput;

        public BoardScoringTests(ITestOutputHelper testOutput)
        {
            this._testOutput = testOutput;
        }

        [Fact]
        public void Evaluate_EmpthBoard_Zeros()
        {
            var board = new SquareBoard(5);
            var subject = GetSubject();

            var blackScore = subject.Evaluate(board, Side.Black);
            var redScore = subject.Evaluate(board, Side.Red);

            Assert.Equal(0, blackScore);
            Assert.Equal(0, redScore);
        }

        [Fact]
        public void Evaluate_SimpleFiguresOnly_BlackSideScore()
        {
            var board = new SquareBoard(5);
            board.Set(Figure.CreateSimple(0, 1, Side.Red));
            board.Set(Figure.CreateSimple(0, 3, Side.Red));
            board.Set(Figure.CreateSimple(1, 2, Side.Red));
            board.Set(Figure.CreateSimple(2, 1, Side.Black));
            var subject = GetSubject();

            var score = subject.Evaluate(board, Side.Black);

            Assert.Equal(1 - 3, score);
        }

        [Fact]
        public void Evaluate_KingsAndSimpleFigures_BlackSideScore()
        {
            var board = new SquareBoard(5);
            board.Set(Figure.CreateSimple(0, 1, Side.Red));
            board.Set(Figure.CreateSimple(0, 3, Side.Red));
            board.Set(Figure.CreateKing(1, 4, Side.Red));

            board.Set(Figure.CreateKing(2, 1, Side.Black));
            board.Set(Figure.CreateKing(2, 3, Side.Black));
            var subject = GetSubject();

            var score = subject.Evaluate(board, Side.Black);

            Assert.Equal((3 + 3) - (1 + 1 + 3), score);
        }

        [Fact]
        public void Evaluate_KingsAndSimpleFigures_RedSideScore()
        {
            var board = new SquareBoard(5);
            board.Set(Figure.CreateSimple(0, 1, Side.Red));
            board.Set(Figure.CreateSimple(0, 3, Side.Red));
            board.Set(Figure.CreateKing(1, 4, Side.Red));

            board.Set(Figure.CreateKing(2, 1, Side.Black));
            board.Set(Figure.CreateKing(2, 3, Side.Black));
            var subject = GetSubject();

            var score = subject.Evaluate(board, Side.Red);

            Assert.Equal((1 + 1 + 3) - (3 + 3), score);
        }

        private IBoardScoring GetSubject()
        {
            return new TrivialBoardScoring();
        }
    }
}
