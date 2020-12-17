using Checkers.Core;
using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Checkers.Core.Tests
{
    public class GameUndoRedoTests
    {
        private readonly ITestOutputHelper testOutput;

        public GameUndoRedoTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Fact]
        public async Task Undo_FirstMove()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);
            stat.Append(null, game); //append initial board as well

            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.UndoMove);
            var player2 = new PresetTestPlayer(GameSide.Red,
                PresetTestPlayer.FirstWalkMove);

            game.Start(player1, player2);
            await game;

            Assert.Equal(5, stat.BoardHistory.Count); //init board + 3 moves + terminal board
            Assert.Equal(stat.BoardHistory[0], game.Board);
            Assert.Equal(0u, game.Turn);
            Assert.Equal(player1, game.CurrentPlayer);
        }

        [Fact]
        public async Task UndoAndRedo_SecondPlayerCantUndoYet()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);
            stat.Append(null, game); //append initial board as well

            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.FirstWalkMove);
            var player2 = new PresetTestPlayer(GameSide.Red,
                PresetTestPlayer.UndoMove);

            game.Start(player1, player2);
            await game;

            Assert.Equal(GameStatus.Error, game.Status);
            Assert.IsType<NoSuchMoveException>(game.Error);
        }

        [Fact]
        public async Task Redo_NoRedoMovesAvailable()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);
            stat.Append(null, game); //append initial board as well

            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.RedoMove);
            var player2 = new PresetTestPlayer(GameSide.Red);

            game.Start(player1, player2);
            await game;

            Assert.Equal(GameStatus.Error, game.Status);
            Assert.IsType<NoSuchMoveException>(game.Error);
        }

        [Fact]
        public async Task Undo_NoUndoMovesAvailable()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);
            stat.Append(null, game); //append initial board as well

            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.UndoMove);
            var player2 = new PresetTestPlayer(GameSide.Red);

            game.Start(player1, player2);
            await game;

            Assert.Equal(GameStatus.Error, game.Status);
            Assert.IsType<NoSuchMoveException>(game.Error);
        }

        [Fact]
        public async Task Redo_WalkMoveShouldBreakFurtherRedoMoves()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);
            stat.Append(null, game); //append initial board as well

            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.UndoMove,
                PresetTestPlayer.UndoMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.RedoMove);
            var player2 = new PresetTestPlayer(GameSide.Red,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove);

            game.Start(player1, player2);
            await game;

            Assert.Equal(GameStatus.Error, game.Status);
            Assert.IsType<NoSuchMoveException>(game.Error);
        }

        [Fact]
        public async Task Undo_AllMoves_EnemyFirstMoveLeft()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);

            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove);
            var player2 = new PresetTestPlayer(GameSide.Red,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.UndoMove,
                PresetTestPlayer.UndoMove);

            game.Start(player1, player2);
            await game;

            Assert.Equal(8, stat.BoardHistory.Count);
            Assert.Equal(stat.BoardHistory[0], game.Board);
            Assert.Equal(1u, game.Turn);
            Assert.Equal(player2, game.CurrentPlayer);
        }

        [Fact]
        public async Task UndoAndRedo_AllMovesUndo_ThenRedoOne()
        {
            var stat = new MoqStatictics();
            var game = GetSubject(gameStatistics: stat);

            //move are 
            var player1 = new PresetTestPlayer(GameSide.Black,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove);
            var player2 = new PresetTestPlayer(GameSide.Red,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.FirstWalkMove,
                PresetTestPlayer.UndoMove,
                PresetTestPlayer.UndoMove,
                PresetTestPlayer.RedoMove);

            game.Start(player1, player2);
            await game;

            Assert.Equal(9, stat.BoardHistory.Count);
            Assert.Equal(stat.BoardHistory[2], game.Board);
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
    }
}
