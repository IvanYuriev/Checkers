using Checkers.Core.Board;
using Checkers.Core.Extensions;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public partial class Game
    {
        private readonly IRules _rulesProvider;
        private readonly IBoardBuilder _boardBuilder;
        private readonly IGameStatistics _gameStatistics;
        private readonly Stack<History> _undoHistory;
        private readonly Stack<History> _redoHistory;
        private readonly IPlayer[] _players = new IPlayer[2];

        private volatile bool _isRunning = false;
        private Task _runningTask;
        private SquareBoard _board;
        private uint _turn;
        private int _winnerIndex;

        private IGameMove UndoMove, RedoMove, StopMove;
        public Game(IRules rulesProvider, IBoardBuilder boardBuilder, IGameStatistics gameStatistics)
        {
            _rulesProvider = rulesProvider;
            _boardBuilder = boardBuilder;
            _undoHistory = new Stack<History>();
            _redoHistory = new Stack<History>();
            _gameStatistics = gameStatistics;
            UndoMove = new UndoGameMove(this);
            RedoMove = new RedoGameMove(this);
            StopMove = new StopGameMove(this);

            _board = _boardBuilder.Build();
        }

        public GameStatus Status { get; private set; }
        public IPlayer CurrentPlayer => _players[CurrentPlayerIndex];
        public IPlayer Winner => _winnerIndex >= 0 ? _players[_winnerIndex] : null;
        public SquareBoard Board => _board;
        public uint Turn => _turn;
        private int CurrentPlayerIndex => (int)_turn % _players.Length;

        public void Start(IPlayer player1, IPlayer player2)
        {
            if (player1.Side == player2.Side) throw new GameException("Players should have different sides");

            StopAndWait();

            _players[0] = player1;
            _players[1] = player2;
            _winnerIndex = -1;
            _undoHistory.Clear();
            _redoHistory.Clear();
            _board = _boardBuilder.Build();
            _turn = 0;
            Status = GameStatus.Started;
            _isRunning = true;

            _runningTask = Task.Run(GameLoop);
            
            //ensure task is running
            var spin = new SpinWait();
            while (_runningTask.Status != TaskStatus.Running) spin.SpinOnce();
        }

        public void Stop()
        {
            _isRunning = false; //TODO: CAS logic probably needed
            Status = GameStatus.Stopped;
        }

        public void StopAndWait(int millisecondsTimeout = 3000)
        {
            Stop();
            Wait(millisecondsTimeout);
        }
        public void Wait(int millisecondsTimeout = 3000)
        {
            if (_runningTask == null) return;
            if (_runningTask.Status != TaskStatus.Running) return;

            _runningTask.Wait(millisecondsTimeout);
        }

        private void GameLoop()
        {
            do
            {
                var validMoves = _rulesProvider.GetMoves(Board, SideUtil.Convert(CurrentPlayer.Side));
                var gameMoves = new List<IGameMove>(validMoves.Values.Count);
                if (validMoves.Count > 0) gameMoves.AddRange(validMoves.Flatten((f, m) => new WalkGameMove(this, f, m)));
                if (_undoHistory.Count > 0) gameMoves.Add(UndoMove);
                if (_redoHistory.Count > 0) gameMoves.Add(RedoMove);

                var playerMoveTask = Task.Run(() => CurrentPlayer.Choose(gameMoves.ToArray(), Board));
                while (Task.WhenAny(playerMoveTask, Task.Delay(300)).Result != playerMoveTask)
                {
                    if (!_isRunning) return; //interrupt player move because Game has Stopped!
                }
                var move = playerMoveTask.Result ?? StopMove;
                move.Execute();

                _gameStatistics.Append(move, this);

                //fire & forget update for players
                Task.Run(() =>
                {
                    foreach (var player in _players) player.GameUpdated(this);
                });
            } while (_isRunning);
        }

        private void CheckForWin()
        {
            // TODO: extract it into Rules class - because there are a lot of draw cases (pretty complex)
            var validEnemyMoves = _rulesProvider.GetMoves(Board, SideUtil.Convert(CurrentPlayer.Side));
            if (validEnemyMoves.Count == 0)
            {
                _winnerIndex = CurrentPlayerIndex;
                Status = _winnerIndex == 0 ? GameStatus.Player1Wins : GameStatus.Player2Wins;
                //TODO: mark game as finished?
                // - then Undo should unmark it?
                // - then to be able to make Redo should store it in History
                // etc.
            }
        }
    }
}
