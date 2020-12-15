using System;

namespace Checkers.Core
{
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
    }

}
