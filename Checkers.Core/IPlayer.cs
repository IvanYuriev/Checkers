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

namespace Checkers.Core
{
    public interface IPlayer
    {
        GameSide Side { get; }
        IGameMove Choose(IGameMove[] moves, SquareBoard board);
        void GameUpdated(Game game);
    }

}
