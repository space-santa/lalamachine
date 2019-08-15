using System.Windows.Controls;
using System.Windows.Input;
using Lalamachine.Wpf.ViewModel;

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
            var context = (LibraryViewModel)DataContext;
            context.SetGenreFilterCommand.Execute(GenreListView.SelectedItem);
        }

        private void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            context.SetArtistFilterCommand.Execute(ArtistListView.SelectedItem);
        }

        private void AlbumListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = (LibraryViewModel)DataContext;
            context.SetAlbumFilterCommand.Execute(AlbumListView.SelectedItem);
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
