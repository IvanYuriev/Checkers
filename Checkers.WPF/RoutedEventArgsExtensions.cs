using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Checkers.WPF
{
    internal static class RoutedEventArgsExtensions
    {
        public static T ButtonTag<T>(this RoutedEventArgs e) where T : class
        {
            var btn = e.Source as Button;
            return btn.Tag as T;
        }
    }
}
