using System;

namespace Checkers.Core.Bot
{
    public partial class NegaMaxBot
    {
        public class BotOptions
        {
            public int MaxDepth { get; set; } = 10;
            public bool IsDebug { get; set; } = false;
            public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount;
        }
    }
}
