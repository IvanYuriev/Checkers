using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Rules;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Checkers.Core.Tests.Bot
{
    public class NegaMaxBotTests
    {
        private readonly ITestOutputHelper testOutput;
        private ILogger<NegaMaxBot> _logger;

        public NegaMaxBotTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutput));
            _logger = loggerFactory.CreateLogger<NegaMaxBot>();
        }

        [Fact]
        public void Int32MinValue_Negation()
        {
            var value = Int32.MinValue + 1;
            Assert.Equal(Int32.MaxValue, -value);
        }

        [Fact]
        public void FindBestMove_PredictEnemyStrike_MoveAnotherFigure()
        {
            /*     0 1 2 3 4
             *   0 . . . . r
                 1 . . . . .
                 2 . . . . b
                 3 . . . . .
                 4 . . . . b   */
            var board = new SquareBoard(5);
            board.Set(Figure.CreateSimple(0, 4, Side.Red));
            board.Set(Figure.CreateSimple(2, 4, Side.Black));
            board.Set(Figure.CreateSimple(4, 4, Side.Black));

            var subject = GetSubject();
            var move = subject.FindBestMove(board, Side.Black, CancellationToken.None, Options(maxDepth: 2));

            Assert.Equal(Figure.CreateSimple(4, 4, Side.Black), move.Figure);
        }

        [Fact]
        public void FindBestMove_ChooseToStrikeKing()
        {
            /*     0 1 2 3 4
                 0 . . . . .
                 1 . r . R .   Simple and King
                 2 . . . . .
                 3 . r . . .
                 4 b . . . .   */
            var board = new SquareBoard(5);
            board.Set(Figure.CreateSimple(1, 1, Side.Red));
            board.Set(Figure.CreateKing(1, 3, Side.Red));
            board.Set(Figure.CreateSimple(3, 1, Side.Red));
            board.Set(Figure.CreateSimple(4, 0, Side.Black));

            var subject = GetSubject();
            var move = subject.FindBestMove(board, Side.Black, CancellationToken.None, Options(maxDepth: 2));

            Assert.Equal(Figure.CreateSimple(4, 0, Side.Black), move.Figure);
            Assert.Equal(1, move.SequenceIndex);
        }

        private IBot GetSubject()
        {
            return new NegaMaxBot(new EnglishDraughtsRules(), new TrivialBoardScoring(), _logger);
        }

        private NegaMaxBot.BotOptions Options(int maxParallel = 0, int maxDepth = 1)
        {
            return new NegaMaxBot.BotOptions { IsDebug = true, DegreeOfParallelism = maxParallel, MaxDepth = maxDepth };
        }

        private static SquareBoard Board6x6Preset1()
        {
            /* 0 1 2 3 4 5 6
             0 . . . . . . .
             1 r . . . . . .
             2 . . . . . . .
             3 b . . . b . .
             4 . . . . . . .
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
    }
}
