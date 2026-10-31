using Avalonia.Controls;
using Avalonia.Interactivity;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Views
{
    public partial class LibraryControl : UserControl
    {
        public LibraryControl()
        {
            InitializeComponent();
        }

        // The lists are replaced whenever a filter changes, which clears their selection. Only a selected item changes the filter.
        private void GenreList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LibraryViewModel context && GenreList.SelectedItem is string genre)
            {
                context.SetGenreFilterCommand.Execute(genre);
            }
        }

        private void ArtistList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LibraryViewModel context && ArtistList.SelectedItem is string artist)
            {
                context.SetArtistFilterCommand.Execute(artist);
            }
        }

        private void AlbumList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LibraryViewModel context && AlbumList.SelectedItem is string album)
            {
                context.SetAlbumFilterCommand.Execute(album);
            }
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (DataContext is not LibraryViewModel context)
            {
                return;
            }

            var text = SearchBox.Text ?? "";
            if (text.Length > 2)
            {
                context.SearchString = text;
            }
            else if (text.Length == 0)
            {
                context.ClearSearchCommand.Execute(null);
            }
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is LibraryViewModel context)
            {
                context.UpdateListsCommand.Execute(null);
            }
        }
    }
}
