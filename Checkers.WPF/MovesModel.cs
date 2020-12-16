using Checkers.Core.Rules;
using static Checkers.Core.Game;

namespace Checkers.WPF
{
    public partial class MainWindow
    {
        public class MovesModel
        {
            public MovesModel(string text, int index, WalkGameMove gameMove)
            {
                Text = text;
                Index = index;
                GameMove = gameMove;
            }

            public string Text { get; }
            public int Index { get; }
            public WalkGameMove GameMove { get; }

            public MoveSequence Sequence => GameMove.MoveSequence;
        }
    }
}
