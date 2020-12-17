
namespace Checkers.Core
{
    public partial class Game
    {
        public class UndoGameMove : IGameMove
        {
            private readonly Game _game;

            internal UndoGameMove(Game game)
            {
                _game = game;
            }
            public void Execute()
            {
                var movesToUndo = _game._players.Length;
                while (movesToUndo > 0)
                {
                    var history = _game._currentHistoryItem.Previous;
                    if (history == null) break;

                    _game._currentHistoryItem = history;
                    _game._board = history.Value.Board;
                    //_game.CheckForWin();
                    _game._turn = history.Value.Turn;
                    movesToUndo--;
                }
            }
        }
    }
}
