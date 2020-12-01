using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Cell> Cells;

        public MainWindow()
        {
            InitializeComponent();

            this.Cells = new ObservableCollection<Cell>()
            {
                new Cell{ Pos = new Point(0, 0), Type = PieceType.Black },
                new Cell{ Pos = new Point(4, 6), Type = PieceType.Red },
                new Cell{ Pos = new Point(7, 7), Type = PieceType.Black },
                new Cell{ Pos = new Point(7, 3), Type = PieceType.Red },
            };

            this.ChessBoard.ItemsSource = this.Cells;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = e.Source as Button;
            var point = b.Tag;
        }
    }
}
