using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lalamachine.ViewModel;

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

        // Control too, the list's own handling of that would select -- all -- along with other items.
        private void FilterList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var list = (ListView)sender;
            if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != 0
                && e.OriginalSource is DependencyObject source
                && ItemsControl.ContainerFromElement(list, source) is ListViewItem { Content: string item })
            {
                e.Handled = true;
                LibraryViewModel.ToggleFilterItem(list.SelectedItems, item);
            }
        }

        private void GenreListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            context.SetGenreFilterCommand.Execute(GenreListView.SelectedItems);
        }

        private void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            context.SetArtistFilterCommand.Execute(ArtistListView.SelectedItems);
        }

        private void AlbumListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            context.SetAlbumFilterCommand.Execute(AlbumListView.SelectedItems);
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            if (SearchBox.Text.Length > 2)
            {
                context.SearchString = SearchBox.Text;
            }
            else if (SearchBox.Text.Length == 0)
            {
                context.ClearSearchCommand.Execute(null);
            }
            else
            {
                return;
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            context.UpdateListsCommand.Execute(null);
        }
    }
}
