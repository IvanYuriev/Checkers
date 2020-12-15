using Checkers.Core.Board;
using Checkers.Core.Extensions;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Checkers.Core
{

    public interface IGameStatistics
    {
        void Append(IGameMove move, Game game);
        void Apply(IStatisticsFormatter formatter);
    }
}
