﻿using Checkers.Core.Board;
using Checkers.Core.Game;
using System.Collections.Generic;

namespace Checkers.Core.Rules
{
    public interface IRules
    {
        GameSide FirstMoveSide { get; }

        bool GameIsOver(SquareBoard board);
        IDictionary<Figure, MoveSequence[]> GetMoves(SquareBoard board, Side side);
    }
}