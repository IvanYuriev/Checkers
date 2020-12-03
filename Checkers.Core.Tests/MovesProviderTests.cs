using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Tests
{
    [TestClass]
    public class MovesProviderTests
    {
        [TestMethod]
        public void GetMoves2x2_RedInTopLeftCorner_HasSingleMove()
        {
            var board = new SquareBoard(2);
            board.Set(Figure.CreateSimple(0, 0, Side.Red));

            var subject = new GameRules();

            //var moves = subject.GetMoves(Side.Red);
        }

       
        private static void Fill(ref SquareBoard board)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((i + j) % 2 == 1) continue;
                    var side = i < 2 ? Side.Red : Side.Black;
                    board.Set(Figure.CreateSimple(i, j, side));
                }
            }
        }
    }
}
