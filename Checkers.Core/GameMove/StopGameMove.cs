using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Core.GameMove
{
    internal class StopGameMove : IGameMove
    {
        private readonly Action _stopAction;

        public StopGameMove(Action stopAction)
        {
            _stopAction = stopAction;
        }
        public void Execute()
        {
            _stopAction();
        }
    }
}
