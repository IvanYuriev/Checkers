using System;

namespace Checkers.Core.Game
{
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
    }

}
