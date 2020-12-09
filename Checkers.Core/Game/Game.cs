using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Rules;
using Checkers.Core.Rules.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Core.Game
{
    public class Game
    {
        private readonly IRules _rulesProvider;
        private readonly IBoardBuilder _boardBuilder;
        private readonly IBot _bot;
        private GameSide _playerSide;
        private IDictionary<Figure, MoveSequence[]> _currentValidMoves;
        private Stack<History> _moveHistory;
        private SquareBoard _board;
        
        public Game(IRules rulesProvider, IBoardBuilder boardBuilder, IBot bot)
        {
            _rulesProvider = rulesProvider;
            _boardBuilder = boardBuilder;
            _bot = bot;
            _moveHistory = new Stack<History>();
            _currentValidMoves = new Dictionary<Figure, MoveSequence[]>();
        }

        public GameSide PlayerSide => _playerSide;
        public GameSide SideMoveNow { get; private set; }
        protected Side CoreSideMoveNow => SideMoveNow == GameSide.Black ? Side.Black : Side.Red;

        public async Task Start(GameSide playerSide)
        {
            _playerSide = playerSide;
            _moveHistory.Clear();
            _board = _boardBuilder.Build();
            _currentValidMoves.Clear();
            SideMoveNow = _rulesProvider.FirstMoveSide;

            if (SideMoveNow != PlayerSide)
                MakeBotRandomMove();

            UpdateAvailableMoves();
            RaiseOnMoveCompleted();
        }

        public GameSide? Winner { get; private set; }

        public void MakeMove(Figure figure, int moveIndex)
        {
            if (Winner.HasValue)
                return;
            if (!_currentValidMoves.TryGetValue(figure, out var moves))
                throw new GameException($"Can't find figure {figure}");
            if (moves.Length <= moveIndex)
                throw new GameException($"Can't find move index for {figure}: {moveIndex}");

            var move = moves[moveIndex];

            // Update History
            _moveHistory.Push(new History { Side = SideMoveNow, Move = move, BoardBeforeMove = Board });

            // DO move
            _board = new MoveCommandChain(figure, Board, move).Execute();

            //UPDATE SCORING!
            if (GameIsOver()) return;
            ToggleSide();

            UpdateAvailableMoves();
            RaiseOnMoveCompleted();
        }

        public void MakeBotRandomMove()
        {
            var rnd = new Random((int)DateTime.UtcNow.Ticks);
            UpdateAvailableMoves();
            var figureIndex = rnd.Next(_currentValidMoves.Count - 1);

            var figure = _currentValidMoves.ElementAt(figureIndex);
            MakeMove(figure.Key, rnd.Next(figure.Value.Length - 1));
        }

        public async Task MakeBotMove(int millisecondsPerMove = 5000)
        {
            var tokenSource = new CancellationTokenSource(millisecondsPerMove);
            var botTask = Task.Factory.StartNew(o =>
            {
                return _bot.FindBestMove(Board, CoreSideMoveNow, tokenSource.Token);
            }, tokenSource.Token);

            var botMove = await botTask;
            if (botMove.Figure == Figure.Nop)
                throw new GameException("Bot can't find a move");
            var currentMoves = _rulesProvider.GetMoves(Board, CoreSideMoveNow);
            if (!currentMoves.TryGetValue(botMove.Figure, out var figureMoves))
                throw new GameException($"Can't find bot figure: {botMove.Figure}");
            if (botMove.SequenceIndex >= figureMoves.Length)
                throw new GameException($"Can't find bot figure move sequence index: {botMove.SequenceIndex}; {botMove.Figure}");

            UpdateAvailableMoves();
            MakeMove(botMove.Figure, botMove.SequenceIndex);
        }

        private bool GameIsOver()
        {
            if (_rulesProvider.GameIsOver(Board))
            {
                Winner = SideMoveNow;
                RaiseOnMoveCompleted();
                return true; //SideMoveNow won!
            }
            return false;
        }

        protected void ToggleSide()
        {
            SideMoveNow = SideMoveNow == GameSide.Black ? GameSide.Red : GameSide.Black;
        }

        public bool Undo()
        {
            if (_moveHistory.Count <= 0) return false;
            if (SideMoveNow != PlayerSide) return false;

            _board = _moveHistory.Pop().BoardBeforeMove;

            UpdateAvailableMoves();
            RaiseOnMoveCompleted();
            return true;
        }

        public IDictionary<Figure, MoveSequence[]> GetValidMoves()
        {
            if (_currentValidMoves == null) UpdateAvailableMoves();
            return new Dictionary<Figure, MoveSequence[]>(_currentValidMoves);
        }

        private void UpdateAvailableMoves()
        {
            _currentValidMoves = _rulesProvider.GetMoves(Board, CoreSideMoveNow);
        }

        public SquareBoard Board => _board;

        public event EventHandler OnMoveCompleted;

        private void RaiseOnMoveCompleted()
        {
            var handler = Volatile.Read(ref OnMoveCompleted);
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

}
