using Checkers.Core.Board;
using System.Collections.Generic;

namespace Checkers.Core.Rules
{
    public interface IRules
    {
        bool GameIsOver();
        IDictionary<Figure, MoveSequence[]> GetMoves(Side side);
    }
}