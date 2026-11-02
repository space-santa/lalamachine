using Avalonia.Controls;
using Avalonia.Interactivity;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Views
{
    public partial class LibraryControl : UserControl
    {
        private bool _listsLoaded;

        public LibraryControl()
        {
            InitializeComponent();
        }

        // A list that gets replaced loses its selection, and selects its item again if the new list has it.
        // So only a selected item changes a filter. The view model clears a filter whose item is gone.
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
                ClearSearch(context);
            }
        }

        private void ClearButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is LibraryViewModel context)
            {
                ClearSearch(context);
            }
        }

        private void ClearSearch(LibraryViewModel context)
        {
            // First, or the replaced lists would select these items again, which would set the filters again.
            GenreList.SelectedItem = null;
            ArtistList.SelectedItem = null;
            AlbumList.SelectedItem = null;
            context.ClearSearchCommand.Execute(null);
        }

        // Loaded fires on every return to the library tab, but the lists only change with a filter or a scan.
        // Rebuilding them would also replace the library playlist, which loses track of the playing track.
        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            if (!_listsLoaded && DataContext is LibraryViewModel context)
            {
                _listsLoaded = true;
                context.UpdateListsCommand.Execute(null);
            }
        }
    }
}
