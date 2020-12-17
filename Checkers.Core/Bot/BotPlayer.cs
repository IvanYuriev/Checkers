using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Linq;
using System.Threading;
using static Checkers.Core.Game;

namespace Checkers.Core.Bot
{
    public class BotPlayer : IPlayer
    {
        private readonly NegaMaxBot _bot;
        private CancellationTokenSource _cts;

        public BotPlayer(GameSide side, IRules rules, IBoardScoring scoring)
        {
            Side = side;
            _bot = new NegaMaxBot(rules, scoring, null);
        }

        public GameSide Side { get; private set; }

        public int TimeoutPerMoveMilliseconds { get; set; } = 5000;

        public IGameMove Choose(IGameMove[] moves, SquareBoard board)
        {
            var walkMoves = moves.Select(x => x as WalkGameMove).Where(x => x != null).ToArray();
            if (walkMoves.Length == 0) return null;

            _cts = new CancellationTokenSource(TimeoutPerMoveMilliseconds);

            var depth = 8 * Math.Pow(TimeoutPerMoveMilliseconds / 1000, 0.4);
            var move = _bot.FindBestMove(board, SideUtil.Convert(Side), _cts.Token, new NegaMaxBot.BotOptions { MaxDepth = (int)depth });

            return walkMoves.FirstOrDefault(x => x.Figure == move.Figure && x.MoveSequence == move.Sequence);
        }

        public void GameUpdated(Game game)
        {
            ; //ignore OR make some pre-processing in parallel
        }

        public void Cancel()
        {
            _cts?.Cancel(); //force cancel
        }
    }
}
