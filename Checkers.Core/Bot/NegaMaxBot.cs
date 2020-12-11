using Checkers.Core.Board;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _totalMovesEstimated;

        public NegaMaxBot(IRules rules, IBoardScoring boardScoring, ILogger<NegaMaxBot> logger)
        {
            _rules = rules;
            _boardScoring = boardScoring;
            _logger = logger;
        }

        public int TotalMovesEstimated
        {
            get { return _totalMovesEstimated; }
            private set { _totalMovesEstimated = value; }
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
        private BotMove Negamax(SquareBoard board, int depth, int alpha, int beta, Side side, CancellationToken branchToken)
        {
            if (cancellation.IsCancellationRequested ||
                !CanSearchDeeper(board, depth))
            {
                return BotMove.Empty(Estimate(board));
            }

            var states = GetStates(board, side);
            if (depth == options.MaxDepth && states.Count == 1)
            {
                var singleState = states[0];
                return new BotMove(singleState.Figure, 0, 0);
            }

            BotMove bestMove = new BotMove(Int32.MinValue);

            var noMoves = true;
            var cts = new CancellationTokenSource();
            var workers = new List<Task>();
            foreach (var state in states)
            {
                noMoves = false;
                if (cts.IsCancellationRequested) break;
                if (branchToken.IsCancellationRequested)
                {
                    cts.Cancel();
                    break;
                }
                
                if (states.Count > 1 && _runningWorkersCount < options.DegreeOfParallelism && depth <= options.MaxDepth - 1)
                {
                    Interlocked.Increment(ref _runningWorkersCount);
                    workers.Add(Task.Run(() =>
                    {
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

            if (noMoves) 
                return BotMove.Empty(Estimate(board));

            return bestMove;

            void DoNegamax(State state)
            {
                if (options.IsDebug) Log("before", board, side, state, bestMove.Score, depth, alpha, beta);
                var boardAfterMove = new MoveCommandChain(state.Figure, state.Board, state.MoveSequence).Execute();
                var score = -Negamax(boardAfterMove, depth - 1, -beta, -alpha, SideUtil.Opposite(side), cts.Token).Score;
                var shouldPrun = false;
                lock (locker)
                {
                    if (score > bestMove.Score)
                    {
                        bestMove = new BotMove(state.Figure, state.SequenceIndex, score);
                    }
                    alpha = Math.Max(alpha, bestMove.Score);
                    //let's move cts.Cancel out of the lock scope
                    shouldPrun = options.AllowPrunning && alpha >= beta; 
                }
                if (options.IsDebug) Log("after", board, side, state, score, depth, alpha, beta);
                if (shouldPrun) cts.Cancel();
            }
        }

        private void Log(string msg, SquareBoard board, Side side, State move, int score, int depth, int alpha, int beta)
        {
            var indent = new String('\t', options.MaxDepth - depth);
            _logger.LogDebug(indent + msg + ":: {side}/{figure}#{index}; bounds: {alpha}/{beta}; score: {score}",
                side, move.Figure, move.SequenceIndex, alpha, beta, score);
        }

        private IList<State> GetStates(SquareBoard board, Side side)
        {
            var moves = _rules.GetMoves(board, side);
            var result = new List<State>(moves.Values.Count);
            foreach (var figureMove in moves)
            {
                for (var i = 0; i < figureMove.Value.Length; i++)
                {
                    result.Add(new State(figureMove.Key, i, board, figureMove.Value[i]));
                }
            }
            return result;
        }

        private bool CanSearchDeeper(SquareBoard board, int depth)
        {
            return depth > 0 && !StackIsNotEnough(depth) && !board.NoFigures(botSide) && !board.NoFigures(playerSide);
        }

        private int Estimate(SquareBoard board)
        {
            Interlocked.Increment(ref _totalMovesEstimated);
            // score > 0 - bot has better board
            // score < 0 - player has better board
            //TODO: append position evaluation - corners and horizontal borders are better
            if (board.NoFigures(botSide)) return -1000;
            if (board.NoFigures(playerSide)) return 1000;

            return _boardScoring.Evaluate(board, botSide);
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

        private struct State
        {
            public Figure Figure { get; private set; }
            public int SequenceIndex { get; private set; }
            public SquareBoard Board { get; private set; }
            public MoveSequence MoveSequence { get; private set; }

            public State(Figure figure, int sequenceIndex, SquareBoard board, MoveSequence moveSequence)
            {
                Figure = figure;
                SequenceIndex = sequenceIndex;
                Board = board;
                MoveSequence = moveSequence;
            }
        }
    }
}
