using Checkers.Core.Board;

namespace Checkers.Core.Bot
{
    public interface IBoardScoring
    {
        int Evaluate(SquareBoard board, Side side);
    }
}
