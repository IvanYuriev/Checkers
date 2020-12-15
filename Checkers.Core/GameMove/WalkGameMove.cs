using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Extensions;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Core.GameMove
{

    public class WalkGameMove : IGameMove
    {
        private Action<Figure, MoveSequence> _action;

        internal WalkGameMove(Action<Figure, MoveSequence> action,
            Figure figure, MoveSequence moveSequence)
        {
            _action = action;
            Figure = figure;
            MoveSequence = moveSequence;
        }

        public Figure Figure { get; }
        public MoveSequence MoveSequence { get; }

        public void Execute()
        {
            _action(Figure, MoveSequence);
        }
    }

}
