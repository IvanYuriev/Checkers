using Checkers.Core;
using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private readonly ManualResetEvent _waitHandler;
        private IPlayer _userPlayer, _botPlayer;

        private ObservableCollection<Cell> Cells;
        private MovesModel SelectedMoveModel;
        private Dictionary<Figure, WalkGameMove[]> AllAvailableMoves;

        public MainWindow()
        {
            _rules = new EnglishDraughtsRules();
            _boardBuilder = new PresetBoardBuilder();// new DraughtsBoardBuilder(); //
            _boardScoring = new TrivialBoardScoring();
            _gameStatistics = new GameStatistics();
            _game = new Game(_rules, _boardBuilder, _gameStatistics);

            _waitHandler = new ManualResetEvent(false);

            InitializeComponent();

            AllAvailableMoves = new Dictionary<Figure, WalkGameMove[]>();
            AvailableMoves = new ObservableCollection<MovesModel>();
            Cells = new ObservableCollection<Cell>();
            lstMoves.ItemsSource = AvailableMoves;
            GameField.DataContext = this;
            ChessBoard.ItemsSource = Cells;
        }

        #region IPlayer implementation
        public GameSide Side { get; set; }

        public IGameMove Choose(IGameMove[] moves, SquareBoard board)
        {
            // 1. exchange moves to categories - Undo, Redo, Walk
            // 2. Walk moves should be used to update AllAvailableMoves

            AllAvailableMoves = moves
                .Where(x => x is WalkGameMove)
                .Cast<WalkGameMove>()
                .GroupBy(x => x.Figure)
                .ToDictionary(key => key.Key, val => val.ToArray());

            _waitHandler.WaitOne(); //wait for the user to choose move
            _waitHandler.Reset();

            var selectedGameMove = SelectedMoveModel?.GameMove;
            App.Current.Dispatcher.Invoke(() =>
            {
                AvailableMoves.Clear();
                SelectedMoveModel = null;
                SelectedFigure = Figure.Nop;
            });

            return selectedGameMove;
        }

        public void GameUpdated(Game game)
        {
            App.Current.Dispatcher.Invoke(RedrawBoard);
        }

        public void Cancel()
        {
            _waitHandler.Set();
        }

        #endregion

        private void RedrawBoard()
        {
            var game = _game;
            if (game.Status != GameStatus.Started)
            {
                //game over
                MessageBox.Show($"Game Is Over, Winner Side: {game.Winner.Side}");
                return;
            }

            UpdateGameBoardCells(game);

            AvailableMoves.Clear();
            SelectedMoveModel = null;
            SelectedFigure = Figure.Nop;
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

        private void StartRed(object sender, RoutedEventArgs e)
        {
            AllAvailableMoves.Clear();
            AvailableMoves.Clear();
            Cells.Clear();

            Side = GameSide.Red;
            _userPlayer = this;
            _botPlayer = new BotPlayer(GameSide.Black, _rules, _boardScoring);
            _game.Start(_userPlayer, _botPlayer);

            RedrawBoard();
        }

        private void StartBlack(object sender, RoutedEventArgs e)
        {
            
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
                if (AllAvailableMoves.TryGetValue(cell.Figure, out var figureMoves) && figureMoves.Length > 0)
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
            if (_game.CurrentPlayer == _userPlayer) _waitHandler.Set();
        }
    }
}
