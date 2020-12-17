
namespace Checkers.Core
{
    public partial class Game
    {
        private class StopGameMove : IGameMove
        {
            private readonly Game _game;

            internal StopGameMove(Game game)
            {
                _game = game;
            }
            public void Execute()
            {
                _game._isRunning = false;
            }
        }
    }
}
