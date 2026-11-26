using System;
using System.Linq;
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

        // Selects the genres of a saved combination, the ones the library still has. All of them: a search that would hide
        // some is cleared, along with the artists and albums selected, as for any other genres. The key and BPM filters stay.
        private void GenreCombination_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Control { DataContext: GenreCombination combination } || DataContext is not LibraryViewModel context)
            {
                return;
            }

            GenreCombinationsButton.Flyout?.Hide();
            if (context.SearchString.Length > 0 || !string.IsNullOrEmpty(SearchBox.Text))
            {
                ClearSearch(context, alsoColumnFilters: false);
            }

            var listed = GenreList.Items.OfType<string>().ToList();
            var indexes = combination.Genres.Select(x => listed.IndexOf(x)).Where(x => x > 0).Order().ToList();
            var selection = GenreList.Selection;
            selection.BeginBatchUpdate();
            try
            {
                selection.Clear();
                // None of them left is no filter.
                selection.Select(indexes.Count > 0 ? indexes[0] : 0);
                foreach (var index in indexes.Skip(1))
                {
                    selection.Select(index);
                }
            }
            finally
            {
                selection.EndBatchUpdate();
            }
            GenreList.ScrollIntoView(indexes.Count > 0 ? indexes[0] : 0);
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (DataContext is not LibraryViewModel context)
            {
                return;
            }

            // Emptied by the user, after a search. Not when it's reported later, after this control emptied it and cleared
            // what it had to, like for a saved genre combination: the genres selected since would be cleared too. Nor when
            // it had too few letters to be searched for.
            var text = SearchBox.Text ?? "";
            if (text.Length > 2)
            {
                context.SearchString = text;
            }
            else if (text.Length == 0 && context.SearchString.Length > 0)
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

        // And the filters of the track list's columns, unless they are to stay.
        private void ClearSearch(LibraryViewModel context, bool alsoColumnFilters = true)
        {
            // First, or the replaced lists would select these items again, which would set the filters again.
            GenreList.UnselectAll();
            ArtistList.UnselectAll();
            AlbumList.UnselectAll();
            if (alsoColumnFilters)
            {
                context.ClearSearchCommand.Execute(null);
            }
            else
            {
                context.ClearSearchAndSelection();
            }
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
