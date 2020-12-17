using Checkers.Core.Board;
using System.Collections.Generic;

namespace Checkers.Core.Tests
{

    public class MoqStatictics : IGameStatistics
    {
        public IList<SquareBoard> BoardHistory = new List<SquareBoard>();
        public void Append(IGameMove move, Game game)
        {
            BoardHistory.Add(game.Board);
        }

        public void Apply(IStatisticsFormatter formatter) { }
    }

}
