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
            LibraryViewModel context = (LibraryViewModel)DataContext;
            context.SetGenreFilterCommand.Execute(GenreListView.SelectedItem);
        }

        private void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LibraryViewModel context = (LibraryViewModel)DataContext;
            context.SetArtistFilterCommand.Execute(ArtistListView.SelectedItem);
        }

        private void AlbumListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LibraryViewModel context = (LibraryViewModel)DataContext;
            context.SetAlbumFilterCommand.Execute(AlbumListView.SelectedItem);
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
