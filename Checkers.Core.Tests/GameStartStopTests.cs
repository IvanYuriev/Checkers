using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Checkers.Core.Tests
{
    public class GameStartStopTests
    {
        private readonly ITestOutputHelper testOutput;

        public GameStartStopTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Fact]
        public async Task Start_BeforeAnyMove_InitialGameState()
        {
            var game = GetSubject();
            var player1 = new PresetTestPlayer(GameSide.Black /*, NO MOVES available */);
            var player2 = new PresetTestPlayer(GameSide.Red /*, NO MOVES available */);

            game.Start(player1, player2);
            await game;

            //Assert.Equal(GameStatus.Stopped, game.Status);
            Assert.Equal(0u, game.Turn);
            Assert.Equal(player1, game.CurrentPlayer);
        }

        [Fact]
        public async Task Start_AfterSingleMove_TurnToSecondPlayer()
        {
            var game = GetSubject();
            var player1 = new PresetTestPlayer(GameSide.Black, PresetTestPlayer.FirstWalkMove);
            var player2 = new PresetTestPlayer(GameSide.Red);

            game.Start(player1, player2);
            await game;

            //Assert.Equal(GameStatus.Stopped, game.Status);
            Assert.Equal(1u, game.Turn);
            Assert.Equal(player2, game.CurrentPlayer);
        }

        [Fact]
        public async Task Start_AfterSecondMove_TurnToFirstPlayerAgain()
        {
            var game = GetSubject();
            var player1 = new PresetTestPlayer(GameSide.Black, PresetTestPlayer.FirstWalkMove);
            var player2 = new PresetTestPlayer(GameSide.Red, PresetTestPlayer.FirstWalkMove);

            game.Start(player1, player2);
            await game;

            //Assert.Equal(GameStatus.Stopped, game.Status);
            Assert.Equal(2u, game.Turn);
            Assert.Equal(player1, game.CurrentPlayer);
        }

        [Fact]
        public async Task Start_SingleJumpMoveExistOnly_RedWon()
        {
            var game = GetSubject(boardBuilder: new PresetBoardBuilder());
            var player1 = new PresetTestPlayer(GameSide.Red, PresetTestPlayer.FirstWalkMove);
            var player2 = new PresetTestPlayer(GameSide.Black);

            game.Start(player1, player2);
            await game;

            Assert.Equal(GameStatus.Player1Wins, game.Status);
            Assert.Equal(1u, game.Turn);
            Assert.Equal(player2, game.CurrentPlayer);
        }

        //TODO: apply few more tests for Start\Stop race conditions and fix Game in tread-safe way

        private Game GetSubject(IRules rules = default, IBoardBuilder boardBuilder = default, IGameStatistics gameStatistics = default)
        {
            if (rules == default) rules = new EnglishDraughtsRules();
            if (boardBuilder == default) boardBuilder = new DraughtsBoardBuilder();
            if (gameStatistics == default) gameStatistics = new GameStatistics();

            return new Game(rules, boardBuilder, gameStatistics);
        }
    }
}
