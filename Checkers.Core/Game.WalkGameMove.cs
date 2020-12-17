using Checkers.Core.Board;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;

namespace Checkers.Core
{
    public partial class Game
    {
        public class WalkGameMove : IGameMove
        {
            private readonly Game _game;
            private readonly Figure _figure;
            private readonly MoveSequence _moveSequence;

            internal WalkGameMove(Game game, Figure figure, MoveSequence moveSequence)
            {
                _game = game;
                _figure = figure;
                _moveSequence = moveSequence;
            }

            public Figure Figure => _figure;
            public MoveSequence MoveSequence => _moveSequence;

            public void Execute()
            {
                _game._board = new MoveCommandChain(_figure, _game.Board, _moveSequence).Execute();

                // remove ability to redo moves after current move
                while (_game._currentHistoryItem != _game._history.Last)
                {
                    _game._history.RemoveLast();
                }
                _game.CheckForWin();
                _game._turn++;
                _game._history.AddLast(new History { Board = _game.Board, Move = _moveSequence, Side = _game.CurrentPlayer.Side, Turn = _game.Turn });
                _game._currentHistoryItem = _game._history.Last;
            }
        }
    }
}
