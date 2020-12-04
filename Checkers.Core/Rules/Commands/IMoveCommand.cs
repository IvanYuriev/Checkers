using Checkers.Core.Board;
using Checkers.Core.Rules;

namespace Checkers.Core.Rules.Commands
{
    public interface IMoveCommand
    {
        Figure CurrentFigure { get; }
        MoveStep Step { set; }
        SquareBoard Execute(SquareBoard board, Figure figure);
    }
}
