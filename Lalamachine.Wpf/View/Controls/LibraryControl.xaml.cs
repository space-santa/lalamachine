using Lalamachine.Wpf.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text.Length < 3) { return; }
            LibraryViewModel context = (LibraryViewModel)DataContext;
            context.UpdateListsCommand.Execute(null);
        }
    }
}
