using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public class MovesProvider
    {
        private readonly SquareBoard board;

        public MovesProvider(SquareBoard board)
        {
            this.board = board;
        }

        public List<Move> GetMoves(Side side)
        {
            var enemySide = SideUtil.Opposite(side);
            var figures = board.GetAll(side);
            var results = new List<Move>(figures.Length * 2); //in average 2 moves for each figure
            foreach(var figure in figures)
            {
                foreach(var moveSteps in figure.Directions)
                {
                    var singleStep = board.Get(moveSteps(1));
                    if (singleStep.Side == enemySide)
                    {
                        var doubleStep = moveSteps(2);
                        if (board.IsEmpty(doubleStep))
                        {
                            results.Add(new Move(figure.Point, doubleStep));
                        }
                    }
                    else if (singleStep.Side == Side.Empty)
                    {
                        results.Add(new Move(figure.Point, singleStep.Point));
                    }
                }
            }
            return results;
        }
    }
}
