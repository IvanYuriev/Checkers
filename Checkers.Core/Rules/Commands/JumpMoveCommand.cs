using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core.Rules.Commands
{
    public class JumpMoveCommand : IMoveCommand
    {
        public Figure CurrentFigure { get; private set; }
        public MoveStep Step { get; set; }

        public SquareBoard Execute(SquareBoard board, Figure figure)
        {
            // jump over the enemy cell
            var initialPosition = figure.Point;
            board.Clear(initialPosition);
            CurrentFigure = new Figure(Step.Target, figure.Side, figure.IsKing);
            board.Set(CurrentFigure);

            // remove enemy piece
            int offsetRow = 1;
            int offsetCol = 1;
            if (initialPosition.Row > Step.Target.Row) offsetRow = -1; //moving upper
            if (initialPosition.Col > Step.Target.Col) offsetCol = -1; //moving left
            var middlePoint = Point.At(initialPosition.Row + offsetRow, initialPosition.Col + offsetCol);
            var enemy = board.Get(middlePoint);
            //TODO: Notify Scoring Logger about removed enemy figure
            board.Clear(middlePoint);
            return board;
        }
    }
}
