using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Lalamachine.ViewModel;
using LibLala.DomainPrimitives;

namespace Lalamachine.Desktop.Views
{
    // What the playlist and the library playlist grids have in common.
    internal static class TrackGrid
    {
        public static void Attach(DataGrid grid)
        {
            grid.Sorting += new Sorter().Sort;
            grid.AddHandler(InputElement.DoubleTappedEvent, Grid_DoubleTapped, handledEventsToo: true);
        }

        // Null outside of the rows, e.g. on the column headers.
        public static PlaylistTags? TrackAt(object? eventSource)
        {
            return (eventSource as Visual)?.FindAncestorOfType<DataGridRow>(includeSelf: true)?.DataContext as PlaylistTags;
        }

        private static void Grid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is DataGrid { DataContext: PlaylistBaseViewModel viewModel } && TrackAt(e.Source) is { } track)
            {
                viewModel.PlayTrackCommand.Execute(track);
            }
        }

        // Sorts the playlist itself, not just the grid's view of it, so that next and previous follow the order on screen.
        private sealed class Sorter
        {
            private DataGridColumn? _lastColumn;
            private ListSortDirection _lastDirection = ListSortDirection.Ascending;

            public void Sort(object? sender, DataGridColumnEventArgs e)
            {
                e.Handled = true;
                // The Tag is the column's name, the header also shows the sort direction.
                if (sender is not DataGrid { DataContext: PlaylistBaseViewModel viewModel } || e.Column.Tag is not string columnName)
                {
                    return;
                }

                var direction = e.Column == _lastColumn && _lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                if (_lastColumn is { Tag: string lastColumnName } && _lastColumn != e.Column)
                {
                    _lastColumn.Header = lastColumnName;
                }
                e.Column.Header = direction == ListSortDirection.Ascending ? $"{columnName} ▲" : $"{columnName} ▼";

                _lastColumn = e.Column;
                _lastDirection = direction;

                viewModel.SortCommand.Execute(new SortMessage(direction, new PlaylistColumn(columnName)));
            }
        }
    }
}
