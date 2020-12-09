using Checkers.Core.Board;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Checkers.Core.Bot
{
    public class MiniMaxBot : IBot
    {
        private readonly IRules _rules;
        private readonly IBoardScoring _boardScoring;
        private Side botSide;
        private Side playerSide;
        private CancellationToken cancellation;
        private const int BOT_LOST_SCORE = Int32.MinValue;
        private const int PLAYER_LOST_SCORE = Int32.MaxValue;
        private const int MAX_DEPTH = 50;

        private object locker = new object();

        public MiniMaxBot(IRules rules, IBoardScoring boardScoring)
        {
            _rules = rules;
            _boardScoring = boardScoring;
        }

        //HOWTO: make it possible to visualize search progress - let WPF to see each State and decisions?

        public BotMove FindBestMove(SquareBoard board, Side botSide, CancellationToken cancellation)
        {
            this.botSide = botSide;
            this.playerSide = SideUtil.Opposite(botSide);
            this.cancellation = cancellation;

            return Negamax(board, MAX_DEPTH, Int32.MinValue, Int32.MaxValue, botSide);
        }


        // NegaMax algorithm with alpha/beta, source: https://en.wikipedia.org/wiki/Negamax
        //TODO: append transposition tables
        //TODO: append parallel with cancellation for alpha/beta logic
        private BotMove Negamax(SquareBoard board, int depth, int alpha, int beta, Side side)
        {
            if (!CanSearchDeeper(ref board, depth))
            {
                return BotMove.Empty(Estimate(ref board));
            }

            var moves = GetOrderedMoves(board, side); // ref board?
            // hot-path is needed here?
            BotMove bestMove = default;
            foreach (var move in moves)
            {
                var score = -Negamax(move.Board, depth - 1, -beta, -alpha, SideUtil.Opposite(side)).Score;
                if (score > bestMove.Score)
                {
                    bestMove = new BotMove(move.Figure, move.SequenceIndex, score);
                }
                alpha = Math.Max(alpha, bestMove.Score);
                if (alpha >= beta) break;
            }
            return bestMove;
        }

        private IEnumerable<Move> GetOrderedMoves(SquareBoard board, Side side)
        {
            var moves = _rules.GetMoves(board, side);
            foreach(var figureMove in moves)
            {
                for(var i = 0; i < figureMove.Value.Length; i++)
                {
                    var boardAfterMove = new MoveCommandChain(figureMove.Key, board, figureMove.Value[i]).Execute();
                    yield return new Move(figureMove.Key, i, boardAfterMove);
                }
            }
        }

        private bool CanSearchDeeper(ref SquareBoard board, int depth)
        {
            return depth > 0 && StackIsNotEnough(depth) && !board.NoFigures(botSide) && !board.NoFigures(playerSide);
        }

        private int Estimate(ref SquareBoard board)
        {
            // score > 0 - bot has better board
            // score < 0 - player has better board
            //TODO: append position evaluation - corners and horizontal borders are better
            return _boardScoring.Evaluate(board, botSide) - _boardScoring.Evaluate(board, playerSide);
        }

        private static bool TryGetFastPathMove(IDictionary<Figure, MoveSequence[]> figures, BotMove lastMove, int score, out BotMove move)
        {
            move = default;
            //if (figures.Count == 0) //no figures to make move - BOT LOST!
            //{
            //    move = new BotMove { Score = score, Figure = lastMove.Figure, MoveIndex = lastMove.MoveIndex };
            //    return true;
            //}
            //if (figures.Count == 1 && figures.First().Value.Length == 1)
            //{
            //    move = new BotMove { Figure = figures.First().Key, MoveIndex = 0 };
            //    return true;
            //}
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

        private struct Move
        {
            public Figure Figure { get; private set; }
            public int SequenceIndex { get; private set; }
            public SquareBoard Board { get; private set; }

            public Move(Figure figure, int sequenceIndex, SquareBoard board)
            {
                Figure = figure;
                SequenceIndex = sequenceIndex;
                Board = board;
            }
        }
    }
}
