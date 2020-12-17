using Checkers.Core;
using Checkers.Core.Rules;
using static Checkers.Core.Game;

namespace Checkers.WPF
{
    public partial class MainWindow
    {
        public class MovesModel
        {
            public MovesModel(string text, int index, IGameMove gameMove)
            {
                Text = text;
                Index = index;
                GameMove = gameMove;
            }

            public string Text { get; }
            public int Index { get; }
            public IGameMove GameMove { get; }

            public MoveSequence Sequence => (GameMove as WalkGameMove)?.MoveSequence;
        }
    }
}
