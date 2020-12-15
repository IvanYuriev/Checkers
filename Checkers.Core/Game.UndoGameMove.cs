
namespace Checkers.Core
{
    public partial class Game
    {
        private class UndoGameMove : IGameMove
        {
            private readonly Game _game;

            internal UndoGameMove(Game game)
            {
                _game = game;
            }
            public void Execute()
            {
                //undo 2 moves to turn on the current player
                var movesToUndo = _game._players.Length;
                while (movesToUndo > 0 && _game._undoHistory.Count > 0)
                {

                    var history = _game._undoHistory.Pop();
                    _game._board = history.BoardBeforeMove;
                    _game._redoHistory.Push(history);
                    _game.CheckForWin();
                    _game._turn--;
                    movesToUndo--;
                }
            }
        }
    }
}
