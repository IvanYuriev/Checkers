using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Checkers.WPF
{
	public class Cell : INotifyPropertyChanged
	{
		private Point _Pos;
		public Point Pos
		{
			get { return this._Pos; }
			set { this._Pos = value; OnPropertyChanged("Pos"); }
		}

		private PieceType _Type;
		public PieceType Type
		{
			get { return this._Type; }
			set { this._Type = value; OnPropertyChanged("Type"); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	
}
