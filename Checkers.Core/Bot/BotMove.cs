using Checkers.Core.Board;
using System;

namespace Checkers.Core.Bot
{
    public struct BotMove
    {
        public static BotMove Empty(int score) => new BotMove(score);

        public BotMove(int score)
        {
            Figure = Figure.Nop;
            SequenceIndex = -1;
            Score = score;
        }

        public BotMove(Figure figure, int sequenceIndex, int score)
        {
            Figure = figure;
            SequenceIndex = sequenceIndex;
            Score = score;
        }

        public Figure Figure { get; private set; }
        public int SequenceIndex { get; private set; }
        public int Score { get; private set; }

        public override int GetHashCode() => (Figure, SequenceIndex).GetHashCode();

        public override bool Equals(object obj) => obj is BotMove m && Equals(m);

        public override string ToString()
        {
            return $"{Figure}#{SequenceIndex}:{Score}";
        }

        public bool Equals(BotMove other) => Figure == other.Figure && SequenceIndex == other.SequenceIndex;

        public static bool operator ==(BotMove left, BotMove right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BotMove left, BotMove right)
        {
            return !(left == right);
        }
    }
}
