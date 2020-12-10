using Checkers.Core.Board;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Core.Bot
{
    public partial class NegaMaxBot : IBot
    {
        private readonly IRules _rules;
        private readonly IBoardScoring _boardScoring;
        private readonly ILogger<NegaMaxBot> _logger;
        private BotOptions options;
        private Side botSide;
        private Side playerSide;
        private CancellationToken cancellation;

        private object locker = new object();
        private int _runningWorkersCount = 0;
        

        public NegaMaxBot(IRules rules, IBoardScoring boardScoring, ILogger<NegaMaxBot> logger)
        {
            _rules = rules;
            _boardScoring = boardScoring;
            _logger = logger;
        }

        //HOWTO: make it possible to visualize search progress - let WPF to see each State and decisions?

        public BotMove FindBestMove(SquareBoard board, Side botSide, CancellationToken cancellation, BotOptions options = default)
        {
            this.options = options ?? new BotOptions();
            this.botSide = botSide;
            this.playerSide = SideUtil.Opposite(botSide);
            this.cancellation = cancellation;

            return Negamax(board, this.options.MaxDepth, Int32.MinValue + 1, Int32.MaxValue, botSide, CancellationToken.None);
        }


        // NegaMax algorithm with alpha/beta, source: https://en.wikipedia.org/wiki/Negamax
        //TODO: append transposition tables
        //TODO: append parallel with cancellation for alpha/beta logic
        private BotMove Negamax(SquareBoard board, int depth, int alpha, int beta, Side side, CancellationToken branchToken)
        {
            if (!CanSearchDeeper(board, depth) || cancellation.IsCancellationRequested)
            {
                return BotMove.Empty(Estimate(board));
            }

            var states = GetStates(board, side);
            // hot-path is needed here?
            BotMove bestMove = new BotMove(Int32.MinValue);

            var cts = new CancellationTokenSource();
            var workers = new List<Task>();
            foreach (var state in states)
            {
                if (branchToken.IsCancellationRequested)
                {
                    cts.Cancel();
                    break;
                }
                if (options.IsDebug) Log(board, side, state, bestMove.Score, depth, alpha, beta);

                if (_runningWorkersCount < options.DegreeOfParallelism)
                {
                    workers.Add(Task.Run(() =>
                    {
                        Interlocked.Increment(ref _runningWorkersCount);
                        DoNegamax(state);
                        Interlocked.Decrement(ref _runningWorkersCount);
                    }));
                }
                else
                {
                    DoNegamax(state);
                }
            }

            Task.WaitAll(workers.ToArray());

            return bestMove;

            void DoNegamax(State state)
            {
                var score = -Negamax(state.Board, depth - 1, -beta, -alpha, SideUtil.Opposite(side), cts.Token).Score;
                var shouldPrun = false;
                lock (locker)
                {
                    if (score > bestMove.Score)
                    {
                        bestMove = new BotMove(state.Figure, state.SequenceIndex, score);
                    }
                    alpha = Math.Max(alpha, bestMove.Score);
                    shouldPrun = alpha >= beta;
                }
                if (shouldPrun) cts.Cancel();
            }
        }

        private void Log(SquareBoard board, Side side, State move, int score, int depth, int alpha, int beta)
        {
            _logger.LogDebug("{depth}: {Side} made a move from {Point} #{Move} with bounds {alpha}/{beta} and score {Score}\r\n {Board}",
                options.MaxDepth - depth, side, move.Figure.Point, move.SequenceIndex, alpha, beta, score, move.Board);
        }

        private IEnumerable<State> GetStates(SquareBoard board, Side side)
        {
            //PERF: check it for consistency - Move should be class or struct?
            var moves = _rules.GetMoves(board, side);
            foreach (var figureMove in moves)
            {
                for (var i = 0; i < figureMove.Value.Length; i++)
                {
                    var boardAfterMove = new MoveCommandChain(figureMove.Key, board, figureMove.Value[i]).Execute();
                    yield return new State(figureMove.Key, i, boardAfterMove);
                }
            }
        }

        private bool CanSearchDeeper(SquareBoard board, int depth)
        {
            return depth > 0 && !StackIsNotEnough(depth) && !board.NoFigures(botSide) && !board.NoFigures(playerSide);
        }

        private int Estimate(SquareBoard board)
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

        private class State
        {
            public Figure Figure { get; private set; }
            public int SequenceIndex { get; private set; }
            public SquareBoard Board { get; private set; }

            public State(Figure figure, int sequenceIndex, SquareBoard board)
            {
                Figure = figure;
                SequenceIndex = sequenceIndex;
                Board = board;
            }
        }
    }
}
