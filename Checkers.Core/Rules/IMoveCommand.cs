using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core.Rules
{
    public interface IMoveCommand
    {
        Figure CurrentFigure { get; }
        MoveStep Step { get; set; }
        SquareBoard Execute(SquareBoard board, Figure figure);
    }
}
