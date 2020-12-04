using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core.Rules.Commands
{
    public class WalkMoveCommand : IMoveCommand
    {
        public Figure CurrentFigure { get; set; }
        public MoveStep Step { get; set; }

        public SquareBoard Execute(SquareBoard board, Figure figure)
        {
            board.Clear(figure.Point);
            CurrentFigure = new Figure(Step.Target, figure.Side, figure.IsKing);
            board.Set(CurrentFigure);
            return board;
        }
    }
}
