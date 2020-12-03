using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Board
{
    public enum Side : byte
    {
        Nop = 0,    //out of the bounds or any other invalid values
        Empty = 1,  
        Black = 2,
        Red = 3
    }
}
