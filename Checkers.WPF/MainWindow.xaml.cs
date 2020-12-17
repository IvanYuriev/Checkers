using Checkers.Core;
using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Checkers.Core.Game;
using Figure = Checkers.Core.Board.Figure;
using Point = Checkers.Core.Board.Point;

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IPlayer
    {
        private readonly Game _game;
        private readonly IRules _rules;
        private readonly IBoardBuilder _boardBuilder;
        private readonly IBoardScoring _boardScoring;
        private readonly IGameStatistics _gameStatistics;
        private readonly AutoResetEvent _waitHandler;
        private IPlayer _userPlayer;
        private BotPlayer _botPlayer;

        private ObservableCollection<Cell> Cells;
        private MovesModel SelectedMoveModel;
        private Dictionary<Figure, WalkGameMove[]> WalkMoves;
        private IGameMove UndoMove;
        private IGameMove RedoMove;

        //================
        //TODO: re-implement this using Commands or even Reactive approach
        //================
        public MainWindow()
        {
            _rules = new EnglishDraughtsRules();
            _boardBuilder = new DraughtsBoardBuilder();
            //_boardBuilder = new PresetBoardBuilder();
            _boardScoring = new TrivialBoardScoring();
            _gameStatistics = new GameStatistics();
            _game = new Game(_rules, _boardBuilder, _gameStatistics);
            _userPlayer = this;
            _waitHandler = new AutoResetEvent(false);

            WalkMoves = new Dictionary<Figure, WalkGameMove[]>();
            AvailableMoves = new ObservableCollection<MovesModel>();
            Cells = new ObservableCollection<Cell>();

            InitializeComponent();

            LayoutRoot.DataContext = this;
            ChessBoard.ItemsSource = Cells;
        }

        

        #region IPlayer implementation
        public GameSide Side { get; set; }

        public IGameMove Choose(IGameMove[] moves, SquareBoard board)
        {
            // 1. exchange moves to categories - Undo, Redo, Walk
            // 2. Walk moves should be used to update AllAvailableMoves

            WalkMoves = moves
                .Where(x => x is WalkGameMove)
                .Cast<WalkGameMove>()
                .GroupBy(x => x.Figure)
                .ToDictionary(key => key.Key, val => val.ToArray());

            UndoMove = moves.FirstOrDefault(x => x is UndoGameMove);
            OnPropertyChanged("UndoMoveAvailable");
            RedoMove = moves.FirstOrDefault(x => x is RedoGameMove);
            OnPropertyChanged("RedoMoveAvailable");

            _waitHandler.Reset();
            _waitHandler.WaitOne(); //wait for the user to choose move

            _botPlayer.TimeoutPerMoveMilliseconds = SecondsPerMove * 1000;
            var selectedGameMove = SelectedMoveModel?.GameMove;
            return selectedGameMove;
        }

        public void GameUpdated(Game game)
        {
            App.Current.Dispatcher.Invoke(RedrawBoard);
        }

        public void Cancel()
        {
            if (_game.CurrentPlayer == _userPlayer) _waitHandler.Set();
        }

        #endregion

        private void RedrawBoard()
        {
            UpdateGameBoardCells(_game);

            AvailableMoves.Clear();
            SelectedMoveModel = null;
            SelectedFigure = Figure.Nop;

            Info = $"Status: {_game.Status}{Environment.NewLine}Turn: {_game.Turn}{Environment.NewLine}Side move now: {_game.CurrentPlayer.Side}";
        }

        private void UpdateGameBoardCells(Game game)
        {
            var playerSide = _userPlayer.Side;
            for (int i = 0; i < game.Board.Size; i++)
            {
                for (int j = 0; j < game.Board.Size; j++)
                {
                    var figure = game.Board.Get(Point.At(i, j));
                    var boardCell = new Cell(figure, playerSide);
                    var cellIndex = i * game.Board.Size + j;
                    if (Cells.Count <= cellIndex) 
                        Cells.Add(boardCell);
                    else
                    {
                        var currentCell = Cells[cellIndex];
                        if (boardCell != currentCell)
                        {
                            currentCell.Figure = boardCell.Figure;
                            currentCell.Active = boardCell.Active;
                        }
                    }
                }
            }
        }

        private async void StartRed(object sender, RoutedEventArgs e)
        {
            await StartGame(GameSide.Red);
        }

        private async void StartBlack(object sender, RoutedEventArgs e)
        {
            await StartGame(GameSide.Black);
        }

        private async Task StartGame(GameSide side)
        {
            WalkMoves.Clear();
            AvailableMoves.Clear();
            Cells.Clear();

            if (_game.Status != GameStatus.None)
            {
                _game.Stop();
                await _game;
            }
            Side = side;
            _botPlayer = new BotPlayer(side == GameSide.Black ? GameSide.Red : GameSide.Black, _rules, _boardScoring);

            if (_userPlayer.Side == GameSide.Red)
                _game.Start(_userPlayer, _botPlayer);
            else
                _game.Start(_botPlayer, _userPlayer);

            RedrawBoard();
        }

        #region Notifiable Properties

        private ObservableCollection<MovesModel> _availableMoves;
        public ObservableCollection<MovesModel> AvailableMoves
        {
            get { return _availableMoves; }
            set { _availableMoves = value; OnPropertyChanged("AvailableMoves"); }
        }

        private Figure _selectedFigure;
        public Figure SelectedFigure
        {
            get { return _selectedFigure; }
            set { _selectedFigure = value; OnPropertyChanged("SelectedFigure"); }
        }

        private int _secondsPerMove = 5;
        public int SecondsPerMove
        {
            get { return _secondsPerMove; }
            set 
            {
                if (value >= 1 && value <= 30)
                    _secondsPerMove = value; 
                OnPropertyChanged("SecondsPerMove"); 
            }
        }

        public bool UndoMoveAvailable => UndoMove != null;
        public bool RedoMoveAvailable => RedoMove != null;

        private string _info;
        public string Info
        {
            get { return _info; }
            set { _info = value; OnPropertyChanged("Info"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void CellSelected(object sender, RoutedEventArgs e)
        {
            var cell = e.ButtonTag<Cell>();
            if (cell.Figure.Side == Core.Board.Side.Empty)
            {
                MakeMove(this, null);
            }
            else
            {
                AvailableMoves.Clear();
                if (WalkMoves.TryGetValue(cell.Figure, out var figureMoves) && figureMoves.Length > 0)
                {
                    foreach (var move in figureMoves.Select((gameMove, i) => new MovesModel($"{i + 1}", i, gameMove)))
                    {
                        AvailableMoves.Add(move);
                    }
                }
            }

            SelectedFigure = cell.Figure;

            foreach (var c in Cells)
            {
                if (c.Type == PieceType.None) c.Active = false;
            }
        }

        private void MoveSelected(object sender, MouseEventArgs e)
        {
            SelectedMoveModel = e.ButtonTag<MovesModel>();
            var selectedMoveSequence = SelectedMoveModel.Sequence;
            var allFigureMovePoints = AvailableMoves.SelectMany(x => x.Sequence).Select(x => x.Target);
            foreach(var point in allFigureMovePoints)
            {
                if (point == Point.Nop) continue;

                var cell = Cells.FirstOrDefault(x => x.Pos == point);
                cell.Active = selectedMoveSequence.Contains(point);
            }
        }

        private void MakeMove(object sender, RoutedEventArgs e)
        {
            _waitHandler.Set();
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            SelectedMoveModel = new MovesModel("undo", -1, UndoMove);
            _waitHandler.Set();
        }

        private void Redo(object sender, RoutedEventArgs e)
        {
            SelectedMoveModel = new MovesModel("redo", -1, RedoMove);
            _waitHandler.Set();
        }
    }
}
