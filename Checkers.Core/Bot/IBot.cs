using Checkers.Core.Board;
using System.Threading;

namespace Checkers.Core.Bot
{
    public interface IBot
    {
        BotMove FindBestMove(SquareBoard board, Side botSide, CancellationToken cancellation);
    }
}