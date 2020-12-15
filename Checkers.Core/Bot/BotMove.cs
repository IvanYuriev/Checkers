using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;

namespace Checkers.Core.Bot
{
    public struct BotMove
    {
        public static BotMove Empty(int score) => new BotMove(score);

        public BotMove(int score)
        {
            Figure = Figure.Nop;
            Sequence = default;
            Score = score;
        }

        public BotMove(Figure figure, MoveSequence sequence, int score)
        {
            Figure = figure;
            Sequence = sequence;
            Score = score;
        }

        public Figure Figure { get; private set; }
        public MoveSequence Sequence { get; private set; }
        public int Score { get; private set; }

        public override int GetHashCode() => (Figure, Sequence).GetHashCode();

        public override bool Equals(object obj) => obj is BotMove m && Equals(m);

        public override string ToString()
        {
            return $"{Figure}:{Sequence}:{Score}";
        }

        public bool Equals(BotMove other) => Figure == other.Figure && Sequence == other.Sequence;

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
