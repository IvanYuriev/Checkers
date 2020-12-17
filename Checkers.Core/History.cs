using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core
{
    internal struct History
    {
        public GameSide Side { get; set; }
        public MoveSequence Move { get; set; }
        public SquareBoard Board { get; set; }

        public uint Turn { get; set; }
    }

}
