using Checkers.Core.Board;
using System.Threading;
using static Checkers.Core.Bot.NegaMaxBot;

namespace Checkers.Core.Bot
{
    public interface IBot
    {
        int TotalMovesEstimated { get; }
        BotMove FindBestMove(SquareBoard board, Side botSide, CancellationToken cancellation, BotOptions options = default);

    }
}