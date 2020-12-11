using Checkers.Core.Board;
using Checkers.Core.Game;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;

namespace Checkers.Core.Tests.Bot
{
    public partial class NegaMaxBotTests
    {
        internal class MockRules : IRules
        {
            private readonly Figure _figure;

            public MockRules(Figure figure)
            {
                _figure = figure;
            }

            public GameSide FirstMoveSide => throw new NotImplementedException();
            public bool GameIsOver(SquareBoard board) => throw new NotImplementedException();

            public IDictionary<Figure, MoveSequence[]> GetMoves(SquareBoard board, Side side)
            {
                //fake move to the same cell; always 2 cases are possible
                return new Dictionary<Figure, MoveSequence[]> 
                {
                    { _figure, new MoveSequence[] 
                        { 
                            new MoveSequence(MoveStep.Move(_figure.Point)), 
                            new MoveSequence(MoveStep.Move(_figure.Point)) 
                        } 
                    }
                };
            }
        }
    }
}
