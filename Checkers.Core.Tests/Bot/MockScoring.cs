using Checkers.Core.Board;
using Checkers.Core.Bot;
using System;

namespace Checkers.Core.Tests.Bot
{
    public partial class NegaMaxBotTests
    {
        internal class MockScoring : IBoardScoring
        {
            private int _index;
            private readonly int[] _scorings;

            public MockScoring(params int[] scorings)
            {
                _scorings = scorings;
                Reset();
            }
            public int Evaluate(SquareBoard board, Side side)
            {
                if (_index >= _scorings.Length) throw new IndexOutOfRangeException();
                return _scorings[_index++];
            }
            public void Reset() { _index = 0; }
        }
    }
}
