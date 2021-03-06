﻿using Checkers.Core.Board;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Checkers.Core.Rules
{
    public class EnglishDraughtsRules : IRules
    {
        private SquareBoard board;

        public GameSide FirstMoveSide => GameSide.Red;

        private static Func<Point, int, Point>[] KingDirections = new[]
        {
            Direction.BottomLeft,
            Direction.BottomRight,
            Direction.UpperLeft,
            Direction.UpperRight,
        };

        private static Func<Point, int, Point>[] BlackDirections = new[]
        {
            Direction.UpperLeft,
            Direction.UpperRight
        };

        private static Func<Point, int, Point>[] RedDirections = new[]
        {
            Direction.BottomLeft,
            Direction.BottomRight
        };

        public bool GameIsOver(SquareBoard board)
        {
            //TODO: need to find more optimal solution
            var blackMoves = GetMoves(board, Side.Black).Count;
            var redMoves = GetMoves(board, Side.Red).Count;

            return blackMoves == 0 || redMoves == 0;
        }

        public IDictionary<Figure, MoveSequence[]> GetMoves(SquareBoard board, Side side)
        {
            this.board = board;
            var figures = board.GetAll(side);
            var jumpMoves = new Dictionary<Figure, MoveSequence[]>(figures.Length + 1);
            var simpleMoves = new Dictionary<Figure, MoveSequence[]>(figures.Length + 1);
            foreach (var figure in figures)
            {
                var jumps = new List<MoveSequence>(4);
                BuildJumpSequences(figure, new MoveSequence(), jumps);
                if (jumps.Count > 0)
                {
                    jumpMoves.Add(figure, jumps.ToArray());
                    continue;
                }

                // jump has a priority - no need to calculate walk moves if jump exists
                if (jumpMoves.Count > 0) continue;

                var simple = new List<MoveSequence>(4);
                BuildSimpleMoves(figure, simple);
                if (simple.Count > 0) //no strike moves! Let's append a simple ones
                {
                    simpleMoves.Add(figure, simple.ToArray());
                    continue;
                }
            }

            //priority!
            if (jumpMoves.Count > 0) return jumpMoves;
            return simpleMoves;
        }

        private void BuildSimpleMoves(Figure figure, List<MoveSequence> simpleMoves)
        {
            foreach (var direction in GetDirections(figure))
            {
                var jump = GetJumpPoint(figure, direction, out var neighbour);
                if (jump == Point.Nop && neighbour.Side == Side.Empty)
                {
                    var sequence = new MoveSequence(MoveStep.Move(neighbour.Point));
                    if (!figure.IsKing && ShouldPromoteKing(new Figure(neighbour.Point, figure.Side, figure.IsKing), board.Size))
                        sequence.Add(MoveStep.King());
                    simpleMoves.Add(sequence);
                }
            }
        }

        private void BuildJumpSequences(Figure figure, MoveSequence sequence, List<MoveSequence> builder)
        {
            if (ShouldPromoteKing(figure, board.Size))
            {
                sequence.Add(MoveStep.King());
                builder.Add(sequence); //abort sequence traversing
                return;
            }

            var endOfSequence = true;
            foreach (var direction in GetDirections(figure))
            {
                var position = GetJumpPoint(figure, direction, out var neighbour);
                if (position == Point.Nop) continue;
                if (sequence.Contains(position)) continue;

                endOfSequence = false;
                var figureAfterJump = new Figure(position, figure.Side, figure.IsKing);
                var newSequence = new MoveSequence(sequence);
                newSequence.Add(MoveStep.Jump(figureAfterJump.Point));
                BuildJumpSequences(figureAfterJump, newSequence, builder);
            }
            if (endOfSequence && !sequence.IsEmpty) builder.Add(sequence);
        }

        private static bool ShouldPromoteKing(Figure figure, int boardSize)
        {
            if (figure.IsKing) return false;
            //TODO: don't like it - is it possible to make it cleaner and reusable?
            if (figure.Side == Side.Black && figure.Point.Row == 0) return true;
            if (figure.Side == Side.Red && figure.Point.Row == boardSize - 1) return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point GetJumpPoint(Figure figure, Func<Point, int, Point> direction, out Figure neighbour)
        {
            neighbour = board.Get(direction(figure.Point, 1));
            if (neighbour.Side == SideUtil.Opposite(figure.Side))
            {
                var doubleStep = direction(figure.Point, 2);
                if (board.IsEmpty(doubleStep))
                    return doubleStep;
            }
            return Point.Nop;
        }

        private static Func<Point, int, Point>[] GetDirections(Figure figure)
        {
            if (figure.IsKing) return KingDirections;
            else if (figure.Side == Side.Red) return RedDirections;
            else if (figure.Side == Side.Black) return BlackDirections;
            else throw new NotImplementedException();
        }
    }

}
