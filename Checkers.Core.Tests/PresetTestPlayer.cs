using Checkers.Core.Board;
using System;
using System.Linq;

namespace Checkers.Core.Tests
{
    public class PresetTestPlayer : IPlayer
    {
        private readonly Func<IGameMove[], IGameMove>[] _chooser;
        private int _index = 0;

        public static IGameMove FirstWalkMove(IGameMove[] moves) => moves.FirstOrDefault(m => m is Game.WalkGameMove);
        public static IGameMove UndoMove(IGameMove[] moves) => moves.FirstOrDefault(m => m is Game.UndoGameMove);
        public static IGameMove RedoMove(IGameMove[] moves) => moves.FirstOrDefault(m => m is Game.RedoGameMove);

        public PresetTestPlayer(GameSide side, params Func<IGameMove[], IGameMove>[] chooser)
        {
            Side = side;
            _chooser = chooser;
        }
        public GameSide Side { get; private set; }
        public IGameMove Choose(IGameMove[] moves, SquareBoard board)
        {
            if (_index >= _chooser.Length) return null; // Stop the game if no moves exists
            return _chooser[_index++](moves) ?? throw new NoSuchMoveException();
        }

        public void GameUpdated(Game game) { }
        public void Cancel() { }
    }

    public class NoSuchMoveException : Exception
    {
        public NoSuchMoveException()
        {

        }
        public NoSuchMoveException(string message) : base(message)
        {
        }

        public NoSuchMoveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
