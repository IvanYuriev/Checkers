using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Checkers.Core.Game
{
    public class Game
    {
        private readonly IRules _rulesProvider;
        private readonly IBoardBuilder _boardBuilder;

        private GameSide _playerSide;
        private IDictionary<Figure, MoveSequence[]> _currentValidMoves;
        private Stack<History> _moveHistory;
        private SquareBoard _board;
        
        public Game(IRules rulesProvider, IBoardBuilder boardBuilder)
        {
            _rulesProvider = rulesProvider;
            _boardBuilder = boardBuilder;
            _moveHistory = new Stack<History>();
            _currentValidMoves = new Dictionary<Figure, MoveSequence[]>();
            Start(GameSide.Black); //by default
        }

        public GameSide PlayerSide => _playerSide;
        public GameSide SideMoveNow { get; private set; }
        protected Side CoreSideMoveNow => SideMoveNow == GameSide.Black ? Side.Black : Side.Red;

        public void Start(GameSide playerSide)
        {
            _playerSide = playerSide;
            _moveHistory.Clear();
            _board = _boardBuilder.Build();
            _currentValidMoves.Clear();
            SideMoveNow = _rulesProvider.FirstMoveSide;
        }

        public void MakeMove(Figure figure, int moveIndex)
        {
            if (!_currentValidMoves.TryGetValue(figure, out var moves))
                throw new GameException($"Can't find figure {figure}");
            if (moves.Length <= moveIndex)
                throw new GameException($"Can't find move index for {figure}: {moveIndex}");

            var move = moves[moveIndex];

            // Update History
            _moveHistory.Push(new History { Side = SideMoveNow, Move = move, BoardBeforeMove = _board });

            // DO move
            var moveHandler = new MoveCommandChain(figure, _board, move);
            _board = moveHandler.Execute();

            //UPDATE SCORING!
            if (_rulesProvider.GameIsOver(Board)) ; //SideMoveNow won!

            //ToggleSide();

            //LET BOT MAKE A MOVE!
            if (_rulesProvider.GameIsOver(Board)) ; //SideMoveNow won!

            //ToggleSide();

            // Reset available moves
            UpdateAvailableMoves();
            RaiseOnMoveCompleted();
        }

        protected void ToggleSide()
        {
            SideMoveNow = SideMoveNow == GameSide.Black ? GameSide.Red : GameSide.Black;
        }

        public bool Undo()
        {
            if (_moveHistory.Count <= 0) return false;
            if (SideMoveNow != PlayerSide) return false;

            var boardBackup = _board;
            while (_moveHistory.Count > 0 && SideMoveNow != PlayerSide)
            {
                var recent = _moveHistory.Pop();
                boardBackup = recent.BoardBeforeMove;
                SideMoveNow = recent.Side;
            }
            _board = boardBackup;
            RaiseOnMoveCompleted();
            return true;
        }

        public IDictionary<Figure, MoveSequence[]> GetValidMoves()
        {
            if (_currentValidMoves == null) UpdateAvailableMoves();
            return new Dictionary<Figure, MoveSequence[]>(_currentValidMoves);
        }

        private void UpdateAvailableMoves()
        {
            _currentValidMoves = _rulesProvider.GetMoves(Board, CoreSideMoveNow);
        }

        public SquareBoard Board => _board;

        public event EventHandler OnMoveCompleted;

        private void RaiseOnMoveCompleted()
        {
            var handler = Volatile.Read(ref OnMoveCompleted);
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

}
