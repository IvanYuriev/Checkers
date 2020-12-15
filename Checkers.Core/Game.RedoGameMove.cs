
namespace Checkers.Core
{
    public partial class Game
    {
        private class RedoGameMove : IGameMove
        {
            private readonly Game _game;

            internal RedoGameMove(Game game)
            {
                _game = game;
            }
            public void Execute()
            {
                var movesToRedo = _game._players.Length;
                while (movesToRedo > 0 && _game._redoHistory.Count > 0)
                {

                    var history = _game._redoHistory.Pop();
                    _game._board = history.BoardBeforeMove;
                    _game._undoHistory.Push(history);
                    _game.CheckForWin();
                    _game._turn++;
                    movesToRedo--;
                }
            }
        }
    }
}
