using Checkers.Core.Board;
using Checkers.Core.Game;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Core.Bot
{
    public class MiniMaxBot : IBot
    {
        private readonly IRules rules;
        private readonly IBoardScoring boardScoring;
        private Side botSide;
        private Side playerSide;
        private CancellationToken cancellation;
        private const int BOT_LOST_SCORE = Int32.MinValue;
        private const int PLAYER_LOST_SCORE = Int32.MaxValue;
        private const int MAX_DEPTH = 50;

        private object locker = new object();

        public MiniMaxBot(IRules rules, IBoardScoring boardScoring)
        {
            this.rules = rules;
            this.boardScoring = boardScoring;
        }

        public BotMove FindBestMove(SquareBoard board, Side botSide, CancellationToken cancellation)
        {
            this.botSide = botSide;
            this.playerSide = SideUtil.Opposite(botSide);
            this.cancellation = cancellation;
            var lastMove = new BotMove { Figure = Figure.Nop, MoveIndex = -1 };
            var bestMove = AlphaBeta(board, 0, lastMove, true, Int32.MinValue, Int32.MaxValue);
            return bestMove;
        }

        private BotMove AlphaBeta(SquareBoard board, int depth, BotMove lastMove, bool isMaximizer, int alpha, int beta)
        {
            if (cancellation.IsCancellationRequested ||
                depth > MAX_DEPTH ||
                StackIsNotEnough(depth)              ||
                board.NoFigures(playerSide) ||
                board.NoFigures(botSide))
            {
                var botScore = boardScoring.Evaluate(board, botSide);
                var playerScore = boardScoring.Evaluate(board, playerSide);
                //var score = isMaximizer ? botScore - playerScore : playerScore - botScore;
                var score = botScore - playerScore;
                return new BotMove { Score = score, Figure = lastMove.Figure, MoveIndex = lastMove.MoveIndex };
            }

            if (isMaximizer)
            {
                int bestScore = int.MinValue;
                var bestMove = default(BotMove);

                var figures = rules.GetMoves(board, botSide);
                if (TryGetFastPathMove(figures, lastMove, BOT_LOST_SCORE, out var m)) return m;
                Parallel.ForEach(figures, (figure, state) =>
                {
                    for (int i = 0; i < figure.Value.Length; i++)
                    {
                        var move = figure.Value[i];
                        var boardAfterMove = new MoveCommandChain(figure.Key, board, move).Execute();
                        var currentMove = new BotMove { Figure = figure.Key, MoveIndex = i };
                        var botMove = AlphaBeta(boardAfterMove, depth + 1, currentMove, !isMaximizer, alpha, beta);
                        currentMove.Score = botMove.Score;
                        lock (locker)
                        {
                            if (botMove.Score > bestScore)
                            {
                                bestScore = botMove.Score;
                                bestMove = currentMove;
                            }
                            alpha = Math.Max(alpha, bestScore);
                            if (alpha >= beta) state.Break(); //TODO: think about it!
                        }
                    }
                });
                return bestMove;
            }
            else //minimizer
            {
                int bestScore = int.MaxValue;
                var bestMove = default(BotMove);

                var figures = rules.GetMoves(board, botSide);
                if (TryGetFastPathMove(figures, lastMove, PLAYER_LOST_SCORE, out var m)) return m;
                foreach (var figure in figures)
                {
                    for (int i = 0; i < figure.Value.Length; i++)
                    {
                        var move = figure.Value[i];
                        var boardAfterMove = new MoveCommandChain(figure.Key, board, move).Execute();
                        var currentMove = new BotMove { Figure = figure.Key, MoveIndex = i };
                        var botMove = AlphaBeta(boardAfterMove, depth + 1, currentMove, !isMaximizer, alpha, beta);
                        currentMove.Score = botMove.Score;
                        //THREADING!
                        if (botMove.Score < bestScore)
                        {
                            bestScore = botMove.Score;
                            bestMove = currentMove;
                        }
                        beta = Math.Min(beta, bestScore);
                        if (alpha >= beta)
                            break;
                    }
                    if (alpha >= beta)
                        break;
                }
                return bestMove;
            }
        }

        private static bool TryGetFastPathMove(IDictionary<Figure, MoveSequence[]> figures, BotMove lastMove, int score, out BotMove move)
        {
            move = default;
            if (figures.Count == 0) //no figures to make move - BOT LOST!
            {
                move = new BotMove { Score = score, Figure = lastMove.Figure, MoveIndex = lastMove.MoveIndex };
                return true;
            }
            if (figures.Count == 1 && figures.First().Value.Length == 1)
            {
                move = new BotMove { Figure = figures.First().Key, MoveIndex = 0 };
                return true;
            }
            return false;
        }

        private bool StackIsNotEnough(int depth)
        {
            if (depth % 10 == 0)
            {
                try
                {
                    RuntimeHelpers.EnsureSufficientExecutionStack();
                }
                catch (InsufficientExecutionStackException)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
