using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkers.Core;
using Checkers.Core.Board;

namespace Checkers.WPF
{
	public class Cell : INotifyPropertyChanged
	{

        public Cell(Figure figure, GameSide playerSide)
        {
            _figure = figure;
			_active = figure.Side == SideUtil.Convert(playerSide);
		}

        public Point Pos
		{
			get { return _figure.Point; }
		}

		private Figure _figure;
		public Figure Figure
		{
			get { return _figure; }
			set { _figure = value; OnPropertyChanged("Figure"); OnPropertyChanged("Pos"); OnPropertyChanged("Type"); }
		}

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

		public override bool Equals(object obj) => obj is Cell cell && cell.Equals(this);

		public bool Equals(Cell other) => this.Figure == other.Figure && this.Active == other.Active;

		public override int GetHashCode() => (_figure, Active).GetHashCode();
    }
}
