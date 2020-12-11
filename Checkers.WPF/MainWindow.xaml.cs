using Checkers.Core;
using Checkers.Core.Board;
using Checkers.Core.Bot;
using Checkers.Core.Game;
using Checkers.Core.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
using Figure = Checkers.Core.Board.Figure;
using Point = Checkers.Core.Board.Point;

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Cell> Cells;
        private Game game;
        private MovesModel SelectedMovesModel;
        private IDictionary<Figure, MoveSequence[]> AllAvailableMoves;
        private GameSide PlayerSide { get; set; } = GameSide.Red;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            _availableMoves = new ObservableCollection<MovesModel>();
            lstMoves.ItemsSource = AvailableMoves;
            GameField.DataContext = this;
            btnPushBot.DataContext = this;
        }

        private void InitializeGame()
        {
            var rules = new EnglishDraughtsRules();
            var boardBuilder = new DraughtsBoardBuilder(); //new PresetBorderBuilder();//
            var boardScoring = new TrivialBoardScoring();
            var ai = new NegaMaxBot(rules, boardScoring, null);
            game = new Game(rules, boardBuilder, ai);
            game.OnMoveCompleted += Game_OnMoveCompleted;
            RedrawBoard();
        }

        private void Game_OnMoveCompleted(object sender, EventArgs e) => RedrawBoard();

        private void RedrawBoard()
        {
            if (game.Winner.HasValue)
            {
                //game over
                MessageBox.Show($"Game Is Over, Winner Side: {game.Winner.Value}");
                return;
            }

            var cells = new ObservableCollection<Cell>();
            var playerSide = game.PlayerSide == GameSide.Black ? Side.Black : Side.Red;
            for (int i = 0; i < game.Board.Size; i++)
            {
                for (int j = 0; j < game.Board.Size; j++)
                {
                    var figure = game.Board.Get(Point.At(i, j));
                    cells.Add(new Cell(figure, playerSide));
                }
            }
            Cells = cells;
            ChessBoard.ItemsSource = Cells;
            _availableMoves?.Clear();
            SelectedMovesModel = null;
            SelectedFigure = Figure.Nop;
            PlayerTurn = game.SideMoveNow == PlayerSide;

            if (PlayerTurn)
            {
                AllAvailableMoves = game.GetValidMoves();
            }
        }

        private async void StartRed(object sender, RoutedEventArgs e)
        {
            PlayerSide = GameSide.Red;
            await game.Start(GameSide.Red);
        }

        private async void StartBlack(object sender, RoutedEventArgs e)
        {
            PlayerSide = GameSide.Black;
            await game.Start(GameSide.Black);
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

        private bool _playerTurn;
        public bool PlayerTurn
        {
            get { return _playerTurn; }
            set { _playerTurn = value; OnPropertyChanged("PlayerTurn"); OnPropertyChanged("BotTurn"); }
        }

        public bool BotTurn => !PlayerTurn;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void CellSelected(object sender, RoutedEventArgs e)
        {
            var cell = e.ButtonTag<Cell>();
            if (cell.Figure.Side == Side.Empty)
            {
                game.MakeMove(SelectedFigure, SelectedMovesModel.Index);
            }
            else
            {
                AvailableMoves.Clear();
                if (AllAvailableMoves.TryGetValue(cell.Figure, out var figureMoves) && figureMoves.Length > 0)
                {
                    foreach (var move in figureMoves.Select((seq, i) => new MovesModel($"{i + 1}", i, seq)))
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
            SelectedMovesModel = e.ButtonTag<MovesModel>();
            foreach (var cell in Cells)
            {
                var availableMoveCell = SelectedMovesModel.Sequence.Contains(cell.Pos);
                if (cell.Type == PieceType.None) cell.Active = availableMoveCell;
            }
        }

        private void MakeMove(object sender, RoutedEventArgs e)
        {
            try
            {
                game.MakeMove(SelectedFigure, SelectedMovesModel.Index);
            }
            catch(GameException gameEx)
            {
                MessageBox.Show(gameEx.Message); //TODO: apply TextBlock
            }
        }

        private async void MakeBotMove(object sender, RoutedEventArgs e)
        {
            try
            {
                await game.MakeBotMove(Int32.Parse(txtSecPerMove.Text) * 1000);
            }
            catch (GameException gameEx)
            {
                MessageBox.Show(gameEx.Message); //TODO: apply TextBlock
            }
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            if (!game.Undo()) MessageBox.Show("No moves to undo!");
        }
    }
}
