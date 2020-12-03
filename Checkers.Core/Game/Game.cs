using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Checkers.Core.Game
{
    public class Game
    {
        private readonly IRules _movesProvider;
        private readonly GameSide _playerSide;
        private IDictionary<Figure, MoveSequence[]> _currentValidMoves;
        private Stack<History> _moveHistory;
        private SquareBoard _board;

        private Game()
        {
            _moveHistory = new Stack<History>();
            SideMoveNow = GameSide.Black;
        }

        public Game(SquareBoard board, GameSide playerSide) : this()
        {
            _board = board;
            _playerSide = playerSide;
            _movesProvider = new EnglishDraughtsRules(new ReadOnlyBoard(ref _board));
        }

        public GameSide PlayerSide => _playerSide;
        public GameSide SideMoveNow { get; private set; }
        protected Side CoreSideMoveNow => SideMoveNow == GameSide.Black ? Side.Black : Side.Red;

        public void MakeMove(Figure figure, int moveIndex)
        {
            if (!_currentValidMoves.TryGetValue(figure, out var moves))
                throw new GameException($"Can't find figure {figure}");

            if (moves.Length <= moveIndex)
                throw new GameException($"Can't find move index for {figure}: {moveIndex}");

            var move = moves[moveIndex];

            //DO Command
            _moveHistory.Push(new History { Side = SideMoveNow, Move = move, BoardBeforeMove = _board });

            var currentFigure = figure;
            foreach (var step in move)
            {
                var position = currentFigure.Point;
                switch (step.Type)
                {
                    case MoveStepTypes.Move:
                        _board.Clear(position);
                        currentFigure = new Figure(step.Target, figure.Side, figure.IsKing);
                        _board.Set(currentFigure);
                        break;
                    case MoveStepTypes.PromoteKing:
                        _board.SetKing(position);
                        break;
                    case MoveStepTypes.Jump:
                        // jump over the enemy cell
                        _board.Clear(position);
                        currentFigure = new Figure(step.Target, figure.Side, figure.IsKing);
                        _board.Set(currentFigure);

                        // remove enemy piece
                        (int row, int col) offset = (1, 1); //moving bottom right
                        if (position.Row > step.Target.Row) offset.row = -1; //moving upper
                        if (position.Col > step.Target.Col) offset.col = -1; //moving left
                        var middlePoint = Point.At(position.Row + offset.row, position.Col + offset.col);

                        //TODO: Notify Scoring about removed enemy figure
                        var enemy = _board.Get(middlePoint);

                        _board.Clear(middlePoint);
                        break;
                }
            }

            //ToggleSide();
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
            //if (_currentValidMoves == null)
                _currentValidMoves = _movesProvider.GetMoves(CoreSideMoveNow);
            return new Dictionary<Figure, MoveSequence[]>(_currentValidMoves);
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
