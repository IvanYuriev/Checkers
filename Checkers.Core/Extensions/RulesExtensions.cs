using Checkers.Core.Board;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Extensions
{
    internal static class RulesExtensions
    {
        public static T[] Flatten<T>(this IDictionary<Figure, MoveSequence[]> moves, Func<Figure, MoveSequence, T> builder)
        {
            return moves.SelectMany(figureMoves => figureMoves.Value
                        .Select(m => builder(figureMoves.Key, m))).ToArray();
        }
    }
}
