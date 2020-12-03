using Checkers.Core.Board;

namespace Checkers.Core.Game
{
    public interface IBoardBuilder
    {
        SquareBoard Build();
    }
}