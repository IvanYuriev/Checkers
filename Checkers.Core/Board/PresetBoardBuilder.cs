﻿using Checkers.Core.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Board
{
    public class PresetBoardBuilder : IBoardBuilder
    {
        public SquareBoard Build()
        {
            var board = new SquareBoard(8);
            board.Set(Figure.CreateSimple(1, 7, Side.Red));
            board.Set(Figure.CreateSimple(1, 3, Side.Red));
            board.Set(Figure.CreateSimple(2, 2, Side.Black));
            //board.Set(Figure.CreateSimple(7, 0, Side.Black));
            return board;
        }
    }
}
