using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core
{
    public enum Side : byte
    {
        Nop = 0,    //out of the bounds or any other invalid values
        Empty = 1,  
        Black = 2,
        Red = 3
    }

    public static class SideUtil
    {
        public static Side Opposite(Side s)
        {
            if (s == Side.Black) return Side.Red;
            if (s == Side.Red) return Side.Black;

            return Side.Nop;
        }
    }
}
