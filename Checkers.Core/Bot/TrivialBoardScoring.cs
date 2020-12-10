using Checkers.Core.Board;

namespace Checkers.Core.Bot
{
    public class TrivialBoardScoring : IBoardScoring
    {
        public int Evaluate(SquareBoard board, Side side)
        {
            var score = 0;
            var figures = board.GetAll(side);
            var size = board.Size - 1;
            for (int i = 0; i < figures.Length; i++)
            {
                var figure = figures[i];
                score += figure.IsKing ? 3 : 1;
            }
            return score;
        }
    }
}
