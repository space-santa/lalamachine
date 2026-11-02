using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Views
{
    public partial class LibraryControl : UserControl
    {
        // Control and Command too, the list's own handling of those would select -- all -- along with other items.
        private const KeyModifiers ToggleModifiers = KeyModifiers.Shift | KeyModifiers.Control | KeyModifiers.Meta;

        private bool _listsLoaded;

        public LibraryControl()
        {
            InitializeComponent();

            // Tunnel, so that the list's own selection handling doesn't get to see these.
            GenreList.AddHandler(PointerPressedEvent, FilterList_PointerPressed, RoutingStrategies.Tunnel);
            ArtistList.AddHandler(PointerPressedEvent, FilterList_PointerPressed, RoutingStrategies.Tunnel);
            AlbumList.AddHandler(PointerPressedEvent, FilterList_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void FilterList_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is ListBox { SelectedItems: { } selection } list
                && (e.KeyModifiers & ToggleModifiers) != 0
                && e.GetCurrentPoint(list).Properties.IsLeftButtonPressed
                && (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true)?.DataContext is string item)
            {
                e.Handled = true;
                LibraryViewModel.ToggleFilterItem(selection, item);
            }
        }

        // A list that gets replaced loses its selection, and selects its items again if the new list has them.
        // So only selected items change a filter. The view model clears the filter of an item that is gone.
        // Not SelectedItems, while the list gets replaced that still has the old items, which would set them as the filter again.
        private void GenreList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LibraryViewModel context && GenreList.Selection.SelectedItems is { Count: > 0 } selection)
            {
                context.SetGenreFilterCommand.Execute(selection);
            }
        }

        private void ArtistList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LibraryViewModel context && ArtistList.Selection.SelectedItems is { Count: > 0 } selection)
            {
                context.SetArtistFilterCommand.Execute(selection);
            }
        }

        private void AlbumList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LibraryViewModel context && AlbumList.Selection.SelectedItems is { Count: > 0 } selection)
            {
                context.SetAlbumFilterCommand.Execute(selection);
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
            GenreList.UnselectAll();
            ArtistList.UnselectAll();
            AlbumList.UnselectAll();
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
