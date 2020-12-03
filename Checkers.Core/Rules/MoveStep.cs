using Checkers.Core.Board;

namespace Checkers.Core.Rules
{
    public struct MoveStep
    {
        public MoveStepTypes Type { get; private set; }
        public Point Target { get; private set; }
        
        public static MoveStep Jump(Point p) => new MoveStep { Type = MoveStepTypes.Jump, Target = p };
        public static MoveStep Move(Point p) => new MoveStep { Type = MoveStepTypes.Move, Target = p };
        public static MoveStep King() => new MoveStep { Type = MoveStepTypes.PromoteKing, Target = Point.Nop };

        public override int GetHashCode() => (Target, Type).GetHashCode();

        public override bool Equals(object obj) => obj is MoveStep m && Equals(m);

        public bool Equals(MoveStep other) => Type == other.Type && Target == other.Target;

        public override string ToString() => $"MoveStep({Type},{Target})";
    }

}
