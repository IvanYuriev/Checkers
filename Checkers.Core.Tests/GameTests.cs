using Checkers.Core;
using Checkers.Core.Board;
using Checkers.Core.GameMove;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Checkers.Core.Tests
{
    public class GameTests
    {
        private readonly ITestOutputHelper testOutput;

        public GameTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Fact]
        public void Start_BeforeAnyMove_InitialGameState()
        {
            var game = GetSubject();
            var player1 = new MoqPlayer(GameSide.Black);
            var player2 = new MoqPlayer(GameSide.Red);

            game.Start(player1, player2);
            game.Wait(1000);

            Assert.Equal(GameStatus.Started, game.Status);
            Assert.Equal(0u, game.Turn);
            Assert.Equal(player1, game.CurrentPlayer);
        }

        [Fact]
        public void Start_AfterSingleMove_TurnToSecondPlayer()
        {
            var game = GetSubject();
            var player1 = new MoqPlayer(GameSide.Black, MoqPlayer.FirstWalkMove);
            var player2 = new MoqPlayer(GameSide.Red);

            game.Start(player1, player2);
            game.Wait();

            Assert.Equal(GameStatus.Started, game.Status);
            Assert.Equal(1u, game.Turn);
            Assert.Equal(player2, game.CurrentPlayer);
        }

        [Fact]
        public void Start_AfterSecondMove_TurnToFirstPlayerAgain()
        {
            var game = GetSubject();
            var player1 = new MoqPlayer(GameSide.Black, MoqPlayer.FirstWalkMove);
            var player2 = new MoqPlayer(GameSide.Red, MoqPlayer.FirstWalkMove);

            game.Start(player1, player2);
            game.Wait();

            Assert.Equal(2u, game.Turn);
            Assert.Equal(player1, game.CurrentPlayer);
        }

        [Fact]
        public void Undo_FirstMove()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);
            stat.Append(null, game); //append initial board as well

            var player1 = new MoqPlayer(GameSide.Black, 
                MoqPlayer.FirstWalkMove);
            var player2 = new MoqPlayer(GameSide.Red,
                MoqPlayer.UndoMove);

            game.Start(player1, player2);
            game.Wait();

            Assert.Equal(4, stat.BoardHistory.Count); //init board + 2 moves + terminal board
            Assert.Equal(stat.BoardHistory[0], game.Board);
            Assert.Equal(0u, game.Turn);
            Assert.Equal(player1, game.CurrentPlayer);
        }

        [Fact]
        public void Undo_AllMoves_EnemyFirstMoveLeft()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);

            var player1 = new MoqPlayer(GameSide.Black,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.FirstWalkMove);
            var player2 = new MoqPlayer(GameSide.Red,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.UndoMove,
                MoqPlayer.UndoMove);

            game.Start(player1, player2);
            game.Wait();

            Assert.Equal(8, stat.BoardHistory.Count);
            Assert.Equal(stat.BoardHistory[0], game.Board);
            Assert.Equal(1u, game.Turn);
            Assert.Equal(player2, game.CurrentPlayer);
        }

        [Fact]
        public void UndoAndRedo_AllMovesUndo_ThenRedoOne()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);

            //move are 
            var player1 = new MoqPlayer(GameSide.Black,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.FirstWalkMove);
            var player2 = new MoqPlayer(GameSide.Red,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.FirstWalkMove,
                MoqPlayer.UndoMove,
                MoqPlayer.UndoMove,
                MoqPlayer.RedoMove);

            game.Start(player1, player2);
            game.Wait();

            Assert.Equal(9, stat.BoardHistory.Count);
            Assert.Equal(stat.BoardHistory[1], game.Board);
            Assert.Equal(3u, game.Turn);
            Assert.Equal(player2, game.CurrentPlayer);
        }

        private Game GetSubject(IRules rules = default, IBoardBuilder boardBuilder = default, IGameStatistics gameStatistics = default)
        {
            if (rules == default) rules = new EnglishDraughtsRules();
            if (boardBuilder == default) boardBuilder = new DraughtsBoardBuilder();
            if (gameStatistics == default) gameStatistics = new GameStatistics();

            return new Game(rules, boardBuilder, gameStatistics);
        }

        private class MoqStatictics : IGameStatistics
        {
            public IList<SquareBoard> BoardHistory = new List<SquareBoard>();
            public void Append(IGameMove move, Game game)
            {
                BoardHistory.Add(game.Board);
            }

            public void Apply(IStatisticsFormatter formatter) { }
        }

        private class MoqPlayer : IPlayer
        {
            private readonly Func<IGameMove[], IGameMove>[] _chooser;
            private int _index = 0;

            public static IGameMove FirstWalkMove(IGameMove[] moves) => moves.FirstOrDefault(m => m is WalkGameMove);
            public static IGameMove UndoMove(IGameMove[] moves) => moves.FirstOrDefault(m => m is Game.UndoGameMove);
            public static IGameMove RedoMove(IGameMove[] moves) => moves.FirstOrDefault(m => m is Game.RedoGameMove);

            public MoqPlayer(GameSide side, params Func<IGameMove[], IGameMove>[] chooser)
            {
                Side = side;
                _chooser = chooser;
            }
            public GameSide Side { get; private set; }
            public IGameMove Choose(IGameMove[] moves, SquareBoard board)
            {
                if (_index >= _chooser.Length) return null;
                return _chooser[_index++](moves);
            }

            public void GameUpdated(Game game) { }
        }
    }
}
