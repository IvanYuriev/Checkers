using Checkers.Core.Board;

namespace Checkers.Core.Bot
{
    public class TrivialBoardScoring : IBoardScoring
    {
        public int Evaluate(SquareBoard board, Side side)
        {
            var enemySide = SideUtil.Opposite(side);

            var score = SideScore(board.GetAll(side)) - SideScore(board.GetAll(enemySide));
            return score;
        }

        //TODO: borders should influence score - as a better positions for a figure
        //TODO: terminal positions scoring
        //TODO: is it possible to estimate draw game?

        private int SideScore(Figure[] figures)
        {
            var score = 0;
            for (int i = 0; i < figures.Length; i++)
            {
                var figure = figures[i];
                score += figure.IsKing ? 3 : 1;
            }
            return score;
        }
    }
}
