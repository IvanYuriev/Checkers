using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Linq;

namespace Checkers.Core.Rules
{
    public class MoveCommandChain
    {
        private readonly Figure figure;
        private readonly SquareBoard initialBoard;
        private readonly IMoveCommand[] moves;

        public MoveCommandChain(Figure figure, SquareBoard board, MoveSequence move)
        {
            this.figure = figure;
            this.initialBoard = board;
            moves = move.Select(step => CreateMoveCommand(step)).ToArray();
        }

        private IMoveCommand CreateMoveCommand(MoveStep step)
        {
            switch (step.Type)
            {
                case MoveStepTypes.Move: return new SimpleMoveCommand { Step = step };
                case MoveStepTypes.Jump: return new JumpMoveCommand { Step = step };
                case MoveStepTypes.PromoteKing: return new PromoteKingMoveCommand { Step = step };
                default:
                    throw new NotImplementedException($"Unknown step type: {step.Type}");
            }
        }

        public SquareBoard Execute()
        {
            var board = initialBoard;
            var currentFigure = figure;
            foreach (var step in moves)
            {
                board = step.Execute(board, currentFigure);
                currentFigure = step.CurrentFigure;
                //TODO: append some logs here
            }
            return board;
        }
    }
}
