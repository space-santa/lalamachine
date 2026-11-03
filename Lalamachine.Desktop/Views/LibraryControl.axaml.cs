using System;
using System.Windows.Input;
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
            foreach (var list in new[] { GenreList, ArtistList, AlbumList })
            {
                list.AddHandler(PointerPressedEvent, FilterList_PointerPressed, RoutingStrategies.Tunnel);
                list.AddHandler(KeyDownEvent, FilterList_KeyDown, RoutingStrategies.Tunnel);
            }
        }

        private void FilterList_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is ListBox { SelectedItems: { } selection } list
                && (e.KeyModifiers & ToggleModifiers) != 0
                && e.GetCurrentPoint(list).Properties.IsLeftButtonPressed
                && FilterItemAt(e.Source) is { } item)
            {
                e.Handled = true;
                LibraryViewModel.ToggleFilterItem(selection, item);
            }
        }

        // The same with the keyboard. The list's own handling can leave nothing selected, which doesn't change the filter.
        private void FilterList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is ListBox { SelectedItems: { } selection }
                && e.Key is (Key.Space or Key.Enter)
                && (e.KeyModifiers & ToggleModifiers) != 0
                && FilterItemAt(e.Source) is { } item)
            {
                e.Handled = true;
                LibraryViewModel.ToggleFilterItem(selection, item);
            }
        }

        private static string? FilterItemAt(object? eventSource)
        {
            return (eventSource as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true)?.DataContext as string;
        }

        private void GenreList_SelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateFilter(GenreList, e, context => context.SetGenreFilterCommand);

        private void ArtistList_SelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateFilter(ArtistList, e, context => context.SetArtistFilterCommand);

        private void AlbumList_SelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateFilter(AlbumList, e, context => context.SetAlbumFilterCommand);

        // A list that gets replaced loses its selection, and selects its items again if the new list has them.
        // So only selected items change a filter. The view model clears the filter of an item that is gone.
        // Not SelectedItems, while the list gets replaced that still has the old items, which would set them as the filter again.
        private void UpdateFilter(ListBox list, SelectionChangedEventArgs e, Func<LibraryViewModel, ICommand> setFilterCommand)
        {
            if (list.SelectedItems is { } selectedItems)
            {
                LibraryViewModel.KeepAllOnItsOwn(selectedItems, e.AddedItems);
            }

            // A range goes on from its anchor. Left on -- all --, the first row, the next range would select that again.
            if (list.Selection is { AnchorIndex: 0, Count: > 0 } model && !model.IsSelected(0))
            {
                model.AnchorIndex = model.SelectedIndex;
            }

            if (DataContext is LibraryViewModel context && list.Selection.SelectedItems is { Count: > 0 } selection)
            {
                setFilterCommand(context).Execute(selection);
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
            // The view model has nothing to clear, and the binding nothing to update, for text too short to have been searched for.
            SearchBox.Clear();
        }

        // Loaded fires on every return to the library tab, but the lists only change with a filter or a scan.
        // Rebuilding them would also replace the library playlist, which loses its sort order.
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
