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


    //TODO: Game object should be active or passive (push or pull interaction with Player object)???
    //TODO: How to limit time of move - for AI, for Player?
    //TODO: include results table and history tracking - list of moves in a proper formatting

    public interface IGameMove
    {
        void Execute();
    }

}
