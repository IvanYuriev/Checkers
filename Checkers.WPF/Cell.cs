using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkers.Core;
using Checkers.Core.Board;
using static Checkers.WPF.MainWindow;

namespace Checkers.WPF
{
	public class Cell : INotifyPropertyChanged
	{
		private readonly Figure _figure;

        public Cell(Figure figure, Side playerSide)
        {
            _figure = figure;
			_active = figure.Side == playerSide;
		}

        public Point Pos
		{
			get { return _figure.Point; }
		}

		public Figure Figure => _figure;

		public PieceType Type
		{
			get 
			{ 
				if (_figure.Side == Side.Black) return _figure.IsKing ? PieceType.BlackKing : PieceType.Black;
				if (_figure.Side == Side.Red) return _figure.IsKing ? PieceType.RedKing : PieceType.Red;
				return PieceType.None;
			}
		}

		private bool _active;
		public bool Active
        {
            get { return _active; }
            set { _active = value; OnPropertyChanged("Active"); }
        }

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
