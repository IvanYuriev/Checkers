using Checkers.Core.Board;

namespace Checkers.Core.Bot
{
    public struct BotMove
    {
        public int Score { get; set; }
        public Figure Figure { get; set; }
        public int MoveIndex { get; set; }
    }
}
