﻿
namespace Checkers.Core.Board
{
    public static class SideUtil
    {
        public static Side Opposite(Side s)
        {
            if (s == Side.Black) return Side.Red;
            if (s == Side.Red) return Side.Black;

            return Side.Nop;
        }

        public static Side Convert(GameSide gameSide) => gameSide == GameSide.Black ? Side.Black : Side.Red;
    }
}
