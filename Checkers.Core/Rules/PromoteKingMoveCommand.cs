using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core.Rules
{
    public class PromoteKingMoveCommand : IMoveCommand
    {
        public Figure CurrentFigure { get; set; }
        public MoveStep Step { get; set; }

        public SquareBoard Execute(SquareBoard board, Figure figure)
        {
            board.SetKing(figure.Point);
            CurrentFigure = figure;
            return board;
        }
    }
}
