using Checkers.Core.Rules;

namespace Checkers.WPF
{
    public partial class MainWindow
    {
        public class MovesModel
        {
            public MovesModel(string text, int index, MoveSequence sequence)
            {
                Text = text;
                Index = index;
                Sequence = sequence;
            }

            public string Text { get; }
            public int Index { get; }
            public MoveSequence Sequence { get; }
        }
    }
}
