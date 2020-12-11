using Checkers.Core.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Game
{
    public class DraughtsBoardBuilder : IBoardBuilder
    {
        public SquareBoard Build()
        {
            var board = new SquareBoard(8);
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    if (i == 3 || i == 4) continue;

                    var side = i < 3 ? Side.Red : Side.Black;
                    if (i % 2 != j % 2) board.Set(Figure.CreateSimple(i, j, side));
                }
            }
            return board;
        }
    }
}
