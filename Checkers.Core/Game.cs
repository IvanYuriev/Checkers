using Checkers.Core.Board;
using Checkers.Core.Extensions;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public partial class Game
    {
        private readonly IRules _rulesProvider;
        private readonly IBoardBuilder _boardBuilder;
        private readonly IGameStatistics _gameStatistics;
        private readonly LinkedList<History> _history;
        private IPlayer[] _players;

        private volatile bool _isRunning = false;
        private Task _runningTask;
        private SquareBoard _board;
        private uint _turn;
        private int _winnerIndex;
        private LinkedListNode<History> _currentHistoryItem;

        private IGameMove UndoMove, RedoMove, StopMove;
        public Game(IRules rulesProvider, IBoardBuilder boardBuilder, IGameStatistics gameStatistics)
        {
            _rulesProvider = rulesProvider;
            _boardBuilder = boardBuilder;
            _history = new LinkedList<History>();
            _gameStatistics = gameStatistics;
            UndoMove = new UndoGameMove(this);
            RedoMove = new RedoGameMove(this);
            StopMove = new StopGameMove(this);

            _board = _boardBuilder.Build();
        }

        public Exception Error { get; private set; }
        public GameStatus Status { get; private set; }
        public IPlayer CurrentPlayer => _players[CurrentPlayerIndex];
        public IPlayer Winner => _winnerIndex >= 0 ? _players[_winnerIndex] : null;
        public SquareBoard Board => _board;
        public uint Turn => _turn;
        private int CurrentPlayerIndex => (int)_turn % _players.Length;

        public void Start(IPlayer player1, IPlayer player2)
        {
            if (player1.Side == player2.Side) throw new GameException("Players should have different sides");

            _players = new IPlayer[] { player1, player2 };
            _winnerIndex = -1;
            _history.Clear();
            _board = _boardBuilder.Build();
            _history.AddLast(new History { Board = Board, Side = CurrentPlayer.Side, Turn = 0 });
            _currentHistoryItem = _history.Last;
            _turn = 0;
            Status = GameStatus.Started;
            _isRunning = true;

            _runningTask = Task.Run(async () => await GameLoop());
        }

        public void Stop()
        {
            _isRunning = false; //TODO: CAS logic probably needed
            if (_players != null)
            {
                foreach (var player in _players) player?.Cancel();
            }
            Status = GameStatus.Stopped;
        }

        private async Task GameLoop()
        {
            do
            {
                var validMoves = _rulesProvider.GetMoves(Board, SideUtil.Convert(CurrentPlayer.Side));
                var gameMoves = new List<IGameMove>(validMoves.Values.Count);
                if (validMoves.Count > 0) gameMoves.AddRange(validMoves.Flatten((f, m) => new WalkGameMove(this, f, m)));
                if (_currentHistoryItem.Previous != null && _currentHistoryItem.Previous.Previous != null)
                    gameMoves.Add(UndoMove);
                if (_currentHistoryItem.Next != null && _currentHistoryItem.Next.Next != null)
                    gameMoves.Add(RedoMove);

                try
                {
                    var move = await TryChooseMove(gameMoves);
                    if (move == null) return; //gameLoop was interrupted

                    move.Execute();
                    _gameStatistics.Append(move, this);
                }
                catch (Exception ex)
                {
                    Error = ex?.InnerException ?? ex;
                    Status = GameStatus.Error;
                    _isRunning = false;
                    break;
                }

                foreach (var player in _players) player.GameUpdated(this);

            } while (_isRunning);
        }

        private async Task<IGameMove> TryChooseMove(List<IGameMove> gameMoves)
        {
            var playerMoveTask = Task.Run(() => CurrentPlayer.Choose(gameMoves.ToArray(), Board));
            while (await Task.WhenAny(playerMoveTask, Task.Delay(300)) != playerMoveTask)
            {
                if (!_isRunning)
                {
                    CurrentPlayer.Cancel();
                    return null; //interrupt player move because Game has Stopped!
                }
            }
            return await playerMoveTask ?? StopMove;
        }

        private void CheckForWin()
        {
            // TODO: extract it into Rules class - because there are a lot of draw cases (pretty complex)
            var validEnemyMoves = _rulesProvider.GetMoves(Board, SideUtil.Opposite(SideUtil.Convert(CurrentPlayer.Side)));
            if (validEnemyMoves.Count == 0)
            {
                _winnerIndex = CurrentPlayerIndex;
                Status = _winnerIndex == 0 ? GameStatus.Player1Wins : GameStatus.Player2Wins;
                //_isRunning = false;
                //TODO: potentially it's ok to undo move after game is over?
                // - don't stop the game with _isRunning = false
                // - then Undo should unmark it?
                // - then to be able to make Redo should store it in History
                // etc.
            }
        }
        public void StopAndWait(int millisecondsTimeout = 3000)
        {
            Stop();
            Wait(millisecondsTimeout);
        }
        public void Wait(int millisecondsTimeout = 3000)
        {
            if (_runningTask == null) return;
            _runningTask.Wait(millisecondsTimeout);
        }

        public TaskAwaiter GetAwaiter()
        {
            return _runningTask.GetAwaiter();
        }
    }
}
