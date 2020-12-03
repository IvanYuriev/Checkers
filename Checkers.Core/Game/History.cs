using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core.Game
{
    internal struct History
    {
        public GameSide Side { get; set; }
        public MoveSequence Move { get; set; }
        public SquareBoard BoardBeforeMove { get; set; }
    }

}
