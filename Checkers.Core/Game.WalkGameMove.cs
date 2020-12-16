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
                _game._undoHistory.Push(new History { BoardBeforeMove = _game.Board, Move = _moveSequence, Side = _game.CurrentPlayer.Side });

                _game._board = new MoveCommandChain(_figure, _game.Board, _moveSequence).Execute();

                _game._redoHistory.Clear(); //no way to redo after making a walk move
                _game.CheckForWin();
                _game._turn++;
            }
        }
    }
}
