using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for LibraryControl.xaml
    /// </summary>
    public partial class LibraryControl : UserControl
    {
        public LibraryControl()
        {
            InitializeComponent();
        }

        private void GenreListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
