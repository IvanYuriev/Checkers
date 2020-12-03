using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.Board
{

    public struct ReadOnlyBoard
    {
        private readonly SquareBoard board;

        public ReadOnlyBoard(SquareBoard board)
        {
            this.board = board;
        }

        public Figure[] GetAll(Side side) => board.GetAll(side);
        public Figure Get(Point p) => board.Get(p);
        public bool IsEmpty(Point p) => board.IsEmpty(p);
        public int Size => board.Size;
    }

}
