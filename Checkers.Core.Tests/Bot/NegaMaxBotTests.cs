using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Rules;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Checkers.Core.Tests.Bot
{
    public partial class NegaMaxBotTests
    {
        private readonly ITestOutputHelper _testOutput;
        private ILogger<NegaMaxBot> _logger;

        public NegaMaxBotTests(ITestOutputHelper testOutput)
        {
            this._testOutput = testOutput;
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
        public void FindBestMove_AviodEnemyStrike_TryMoveAnotherFigure()
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
            Assert.Equal(new MoveSequence(MoveStep.Jump(2, 2), MoveStep.Jump(0, 4), MoveStep.King()), move.Sequence);
        }

        [Fact(Skip = "Depends on parallelism factor")]
        public void FindBestMove_RunTheSameBoard_ProduceTheSameResults()
        {
            var board = new SquareBoard(6);
            board.Set(Figure.CreateKing(1, 0, Side.Black));
            board.Set(Figure.CreateKing(1, 2, Side.Black));
            board.Set(Figure.CreateKing(3, 0, Side.Red));

            var subject = GetSubject();
            var options = new NegaMaxBot.BotOptions { IsDebug = false, DegreeOfParallelism = 4, MaxDepth = 10, AllowPrunning = true };
            var moves = Enumerable.Range(0, 50)
                .Select(_ =>
                    subject.FindBestMove(board, Side.Red, CancellationToken.None, options))
                .ToList();

            moves.ForEach(x => _testOutput.WriteLine(x.ToString()));
            var groupingMove = Assert.Single(moves.GroupBy(x => x)); //all move are the same!
            Assert.Equal(Figure.CreateKing(3, 0, Side.Red), groupingMove.Key.Figure);
        }

        [Fact]
        public void FindBestMove_Pruning()
        {
            var blackPiece = Figure.CreateKing(2, 2, Side.Black);
            var redPiece = Figure.CreateKing(0, 0, Side.Red);
            var board = new SquareBoard(3);
            board.Set(blackPiece);
            board.Set(redPiece);
            
            /*      b           MAX
             *    /   \
             *   r     r        MIN
             *  / \   / x
             * 2   4 -5 (any) 
            */
            var scoringMock = new MockScoring(2, 4, -5); //ABSENT value for the 4th scoring => should throw an Exception
            var subject = GetSubject(new MockRules(blackPiece), scoringMock);
            var options = Options(maxParallel: 0, maxDepth: 2);

            options.AllowPrunning = true;
            var move = subject.FindBestMove(board, Side.Black, CancellationToken.None, options);
            Assert.Equal(2, move.Score);

            scoringMock.Reset();
            options.AllowPrunning = false;
            Assert.Throws<IndexOutOfRangeException>(() => subject.FindBestMove(board, Side.Black, CancellationToken.None, options));
        }

        [Fact]
        public void FindBestMove_SingleMoveExists_FastPathWithNoEvaluation()
        {
            /*    0 1 2 3 4
                0 . . . . .
                1 . R . . .   
                2 . . B . B
                3 . . . . .
                4 . . . . .   */
            var board = new SquareBoard(5);
            board.Set(Figure.CreateKing(2, 2, Side.Black));
            board.Set(Figure.CreateKing(2, 4, Side.Black));
            board.Set(Figure.CreateKing(1, 1, Side.Red));

            var subject = GetSubject();
            var options = Options(maxParallel: 2, maxDepth: 3);

            var move = subject.FindBestMove(board, Side.Black, CancellationToken.None, options);
            Assert.Equal(0, subject.TotalMovesEstimated);
            Assert.Equal(Point.At(2, 2), move.Figure.Point);
            Assert.Equal(new MoveSequence(MoveStep.Jump(0, 0)), move.Sequence);
        }

        [Fact]
        public void FindBestMove_ParallelProcessing_EstimationsCheckSum()
        {
            /*     0 1 2 3 4
                 0 r . r . r
                 1 . r . r .   
                 2 . . . . .
                 3 . b . b .
                 4 b . b . b   */
            var board = new SquareBoard(5);
            board.Set(Figure.CreateSimple(0, 0, Side.Red));
            board.Set(Figure.CreateSimple(0, 2, Side.Red));
            board.Set(Figure.CreateSimple(0, 4, Side.Red));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));
            board.Set(Figure.CreateSimple(1, 3, Side.Red));

            board.Set(Figure.CreateSimple(3, 1, Side.Black));
            board.Set(Figure.CreateSimple(3, 3, Side.Black));
            board.Set(Figure.CreateSimple(4, 0, Side.Black));
            board.Set(Figure.CreateSimple(4, 2, Side.Black));
            board.Set(Figure.CreateSimple(4, 4, Side.Black));

            var subject = GetSubject();

            subject.FindBestMove(board, Side.Black, CancellationToken.None, Options(maxParallel: 4, maxDepth: 1));
            Assert.Equal(4, subject.TotalMovesEstimated);

            subject.FindBestMove(board, Side.Black, CancellationToken.None, Options(maxParallel: 4, maxDepth: 2));
            Assert.Equal(4 + 3 + 1 + 1 + 3, subject.TotalMovesEstimated);
        }

        [Fact]
        [Trait("Category", "TimeoutTest")]
        public void FindBestMove_ProcessingCancellation_ExtraTimeLessThanSeccond()
        {
            var boardBuilder = new DraughtsBoardBuilder();
            var board = boardBuilder.Build();
            var turnTimeLimitMs = 2000;

            var subject = GetSubject();
            var results = new List<(int depth, long timeMs, int estimations)>();
            for(int depth = 4; depth <= 20; depth += 4)
            {
                var watch = Stopwatch.StartNew();
                var cts = new CancellationTokenSource(turnTimeLimitMs);
                subject.FindBestMove(board, Side.Black, cts.Token, Options(maxParallel: 4, maxDepth: depth, isDebug: false));
                watch.Stop();
                results.Add((depth, watch.ElapsedMilliseconds, subject.TotalMovesEstimated));
            }

            results.ForEach(x => _testOutput.WriteLine(x.ToString()));
            var maxTimeMs = results.Average(x => x.timeMs);
            Assert.InRange(maxTimeMs - turnTimeLimitMs, Int32.MinValue, 1000);
        }

        private IBot GetSubject(IRules rules = default, IBoardScoring scoring = default)
        {
            if (rules == default) rules = new EnglishDraughtsRules();
            if (scoring == default) scoring = new TrivialBoardScoring();
            return new NegaMaxBot(rules, scoring, _logger);
        }

        private NegaMaxBot.BotOptions Options(bool isDebug = true,int maxParallel = 0, int maxDepth = 1)
        {
            return new NegaMaxBot.BotOptions { IsDebug = isDebug, DegreeOfParallelism = maxParallel, MaxDepth = maxDepth };
        }
    }
}
