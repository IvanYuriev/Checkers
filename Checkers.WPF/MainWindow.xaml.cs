using Checkers.Core;
using Checkers.Core.Board;
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

        public GameSide PlayerSide => GameSide.Black;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            _availableMoves = new ObservableCollection<MovesModel>();
            lstMoves.ItemsSource = AvailableMoves;
        }

        private void InitializeGame()
        {
            SquareBoard board = new SquareBoard(8);
            board.Set(Figure.CreateSimple(1, 3, Side.Red));
            board.Set(Figure.CreateSimple(1, 1, Side.Red));
            board.Set(Figure.CreateSimple(3, 5, Side.Red));
            board.Set(Figure.CreateSimple(1, 5, Side.Red));
            board.Set(Figure.CreateSimple(2, 4, Side.Red));
            board.Set(Figure.CreateKing(4, 6, Side.Black));
            board.Set(Figure.CreateSimple(2, 2, Side.Black));
            //for (int i = 0; i < board.Size; i++)
            //{
            //    for (int j = 0; j < board.Size; j++)
            //    {
            //        if (i == 3 || i == 4) continue;

            //        var side = i < 3 ? Core.Side.Red : Side.Black;
            //        if (i % 2 != j % 2) board.Set(Figure.CreateSimple(i, j, side));
            //    }
            //}
            game = new Game(board, PlayerSide);
            game.OnMoveCompleted += Game_OnMoveCompleted;

            RedrawBoard();
        }

        private void Reset()
        {
            if (game == null) return;
            game.OnMoveCompleted -= Game_OnMoveCompleted;

            InitializeGame();
        }

        private void Game_OnMoveCompleted(object sender, EventArgs e)
        {
            // Entire redrawing all the cells - not optimal, but not a frequent as well
            RedrawBoard();
        }

        private void RedrawBoard()
        {
            var cells = new ObservableCollection<Cell>();
            var playerSide = PlayerSide == GameSide.Black ? Side.Black : Side.Red;
            for (int i = 0; i < game.Board.Size; i++)
            {
                for (int j = 0; j < game.Board.Size; j++)
                {
                    var figure = game.Board.Get(Point.At(i, j));
                    cells.Add(new Cell(figure, playerSide));
                }
            }
            Cells = cells;
            this.ChessBoard.ItemsSource = this.Cells;
            AllAvailableMoves = game.GetValidMoves();
        }

        public class MovesModel
        {
            public string Text { get; set; }
            public int Index { get; set; }
            public MoveSequence Sequence { get; set; }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = e.Source as Button;
            var cell = b.Tag as Cell;

            if (cell.Figure.Side == Side.Empty)
            {
                //MakeMove
                game.MakeMove(SelectedFigure, SelectedMovesModel.Index);
            }
            else
            {
                AvailableMoves.Clear();
                if (AllAvailableMoves.TryGetValue(cell.Figure, out var figureMoves) && figureMoves.Length > 0)
                {
                    foreach (var move in figureMoves.Select((x, i) => new MovesModel { Text = x.ToString(), Index = i, Sequence = x }))
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

        private ObservableCollection<MovesModel> _availableMoves;
        public ObservableCollection<MovesModel> AvailableMoves
        {
            get { return _availableMoves; }
            set { _availableMoves = value; OnPropertyChanged("AvailableMoves"); }
        }

        private Figure _selectedFigure;
        private MovesModel SelectedMovesModel;

        public Figure SelectedFigure
        {
            get { return _selectedFigure; }
            set { _selectedFigure = value; OnPropertyChanged("SelectedFigure"); }
        }

        public IDictionary<Figure, MoveSequence[]> AllAvailableMoves { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var b = e.Source as Button;
            SelectedMovesModel = b.Tag as MovesModel;

            foreach (var c in Cells)
            {
                var availableMoveCell = SelectedMovesModel.Sequence.Contains(c.Pos);
                if (c.Type == PieceType.None) c.Active = availableMoveCell;
            }
        }
    }
}
