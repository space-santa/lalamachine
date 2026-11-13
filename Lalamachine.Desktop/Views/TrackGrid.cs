using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Lalamachine.ViewModel;
using LibLala.DomainPrimitives;

namespace Lalamachine.Desktop.Views
{
    // What the grids of tracks have in common.
    internal static class TrackGrid
    {
        private const string AscendingIndicator = " ▲";
        private const string DescendingIndicator = " ▼";

        // One for all grids, see ColumnOrderSettings.
        private static readonly Lazy<ColumnOrderSettings> ColumnOrders = new(() => new ColumnOrderSettings());

        public static void Attach(DataGrid grid)
        {
            grid.Sorting += Sort;
            // Another playlist in the same grid isn't sorted by what the last one was.
            grid.DataContextChanged += (sender, e) => ClearSortIndicators(grid);
            grid.AddHandler(InputElement.DoubleTappedEvent, Grid_DoubleTapped, handledEventsToo: true);
            AddPlaylistChoices(grid);
        }

        // Lets the user drag the tracks to change their order.
        public static void EnableReorder(DataGrid grid)
        {
            new Reorderer(grid).Attach();
        }

        // Null outside of the rows, e.g. on the column headers.
        private static PlaylistTags? TrackAt(object? eventSource)
        {
            return (eventSource as Visual)?.FindAncestorOfType<DataGridRow>(includeSelf: true)?.DataContext as PlaylistTags;
        }

        // On a header or on the empty part of the row of headers, right of the last column.
        private static bool IsOnColumnHeaders(object? eventSource)
        {
            return (eventSource as Visual)?.FindAncestorOfType<DataGridColumnHeadersPresenter>(includeSelf: true) is not null;
        }

        // The heart of a track is for clicking, not for playing or dragging the track.
        private static bool IsOnButton(object? eventSource)
        {
            return (eventSource as Visual)?.FindAncestorOfType<Button>(includeSelf: true) is not null;
        }

        private static void Grid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is DataGrid { DataContext: PlaylistBaseViewModel viewModel } && !IsOnButton(e.Source) && TrackAt(e.Source) is { } track)
            {
                viewModel.PlayTrackCommand.Execute(track);
            }
        }

        // Shows the columns in the order the user dragged them to the last time. Right-clicking the headers has the default
        // order back. Every grid has its own order, under its name.
        public static void RememberColumnOrder(DataGrid grid, string gridName)
        {
            // The one in the XAML.
            var defaultOrder = grid.Columns.Select(ColumnName).ToList();
            Arrange(grid, ColumnOrders.Value.OrderOf(gridName, defaultOrder));
            // Only after the user dragged a column, not when the order is set here.
            grid.ColumnReordered += (sender, e) => ColumnOrders.Value.SetOrderOf(gridName, ColumnOrder(grid), defaultOrder);

            var restoreItem = new MenuItem { Header = "Restore default column order" };
            restoreItem.Click += (sender, e) =>
            {
                Arrange(grid, defaultOrder);
                ColumnOrders.Value.ResetOrderOf(gridName);
            };
            var menu = new ContextMenu { Items = { restoreItem } };
            menu.Opening += (sender, e) => restoreItem.IsEnabled = !ColumnOrder(grid).SequenceEqual(defaultOrder);
            // On the row of headers, not on each header: the empty part right of the last column is a header that is
            // disabled, a click there goes through it. With a background, the row gets it, not the grid behind it.
            grid.Styles.Add(new Style(x => x.OfType<DataGridColumnHeadersPresenter>())
            {
                Setters =
                {
                    new Setter(Control.ContextMenuProperty, menu),
                    new Setter(Panel.BackgroundProperty, Brushes.Transparent),
                },
            });
        }

        // The Tag is the column's name. A sorted column's header also shows the direction.
        private static string ColumnName(DataGridColumn column)
        {
            return column.Tag as string ?? throw new InvalidOperationException("A column of tracks has no name in its Tag.");
        }

        private static List<string> ColumnOrder(DataGrid grid)
        {
            return grid.Columns.OrderBy(x => x.DisplayIndex).Select(ColumnName).ToList();
        }

        private static void Arrange(DataGrid grid, IReadOnlyList<string> order)
        {
            var columns = grid.Columns.ToDictionary(ColumnName);
            // Moving a column to a place shifts the ones from there on, not the ones before it.
            for (var i = 0; i < order.Count; i++)
            {
                columns[order[i]].DisplayIndex = i;
            }
        }

        public static void ClearSortIndicators(DataGrid grid)
        {
            foreach (var column in grid.Columns)
            {
                // Only the headers with a direction: the name of the ♥ column isn't its header, it has none.
                if (column.Header is string header && (header.EndsWith(AscendingIndicator, StringComparison.Ordinal) || header.EndsWith(DescendingIndicator, StringComparison.Ordinal)))
                {
                    column.Header = ColumnName(column);
                }
            }
        }

        // Sorts the playlist itself, not just the grid's view of it, so that next and previous follow the order on screen.
        private static void Sort(object? sender, DataGridColumnEventArgs e)
        {
            e.Handled = true;
            if (sender is not DataGrid { DataContext: PlaylistBaseViewModel viewModel } grid || e.Column.Tag is not string columnName)
            {
                return;
            }

            var ascendingHeader = columnName + AscendingIndicator;
            var direction = ascendingHeader.Equals(e.Column.Header) ? ListSortDirection.Descending : ListSortDirection.Ascending;

            ClearSortIndicators(grid);
            e.Column.Header = direction == ListSortDirection.Ascending ? ascendingHeader : columnName + DescendingIndicator;

            viewModel.SortCommand.Execute(new SortMessage(direction, new PlaylistColumn(columnName)));
        }

        private static void AddPlaylistChoices(DataGrid grid)
        {
            var menuItem = new MenuItem { Header = "Add to playlist..." };
            menuItem.Click += (sender, e) => ShowPlaylistChoices(grid);
            if (grid.ContextMenu is { } contextMenu)
            {
                contextMenu.Items.Insert(0, menuItem);
                // A track that is outside the library can only be in the queue.
                contextMenu.Opening += (sender, e) =>
                {
                    var tracks = SelectedTracks(grid);
                    menuItem.IsEnabled = tracks.Count > 0 && !tracks.Any(x => x.IsOutsideLibrary);
                };
            }
        }

        // Right-clicking a row selects it, unless it is one of several that already are.
        private static List<PlaylistTags> SelectedTracks(DataGrid grid)
        {
            return grid.SelectedItems.OfType<PlaylistTags>().ToList();
        }

        private static void ShowPlaylistChoices(DataGrid grid)
        {
            if (grid.DataContext is not PlaylistBaseViewModel viewModel)
            {
                return;
            }

            var flyout = new Flyout
            {
                Content = new TrackPlaylistsControl { DataContext = viewModel.TrackPlaylists.ChoicesFor(SelectedTracks(grid)) },
            };
            flyout.ShowAt(grid, showAtPointer: true);
        }

        private sealed class Reorderer
        {
            private static readonly DataFormat<PlaylistTags> TrackFormat = DataFormat.CreateInProcessFormat<PlaylistTags>("lalamachine-playlist-track");

            private readonly DataGrid _grid;
            private PointerPressedEventArgs? _dragStart;

            public Reorderer(DataGrid grid)
            {
                _grid = grid;
            }

            public void Attach()
            {
                DragDrop.SetAllowDrop(_grid, true);
                // Tunnel, so that the grid's own selection handling doesn't hide these.
                _grid.AddHandler(InputElement.PointerPressedEvent, PointerPressed, RoutingStrategies.Tunnel);
                _grid.AddHandler(InputElement.PointerMovedEvent, PointerMoved, RoutingStrategies.Tunnel);
                _grid.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, RoutingStrategies.Tunnel);
                _grid.AddHandler(DragDrop.DragOverEvent, DragOver);
                _grid.AddHandler(DragDrop.DropEvent, Drop);
            }

            private void PointerPressed(object? sender, PointerPressedEventArgs e)
            {
                var isLeftButton = e.GetCurrentPoint(_grid).Properties.IsLeftButtonPressed;
                _dragStart = isLeftButton && !IsOnButton(e.Source) && TrackAt(e.Source) is not null ? e : null;
            }

            private void PointerReleased(object? sender, PointerReleasedEventArgs e)
            {
                _dragStart = null;
            }

            private async void PointerMoved(object? sender, PointerEventArgs e)
            {
                if (_dragStart is null || TrackAt(_dragStart.Source) is not { } track)
                {
                    return;
                }

                // A click wobbles a little, a drag moves further.
                var distance = e.GetPosition(_grid) - _dragStart.GetPosition(_grid);
                if (Math.Abs(distance.X) < 4 && Math.Abs(distance.Y) < 4)
                {
                    return;
                }

                var dragStart = _dragStart;
                _dragStart = null;
                using var data = new DataTransfer();
                var item = DataTransferItem.Create(TrackFormat, track);
                // An in-process format doesn't go on the macOS pasteboard, and a drag without anything on it ends the app:
                // "There must be 1 draggingItem per pasteboardItem". The path as text is something for it.
                item.SetText(track.Path);
                data.Add(item);
                await DragDrop.DoDragDropAsync(dragStart, data, DragDropEffects.Move).ConfigureAwait(true);
            }

            private void DragOver(object? sender, DragEventArgs e)
            {
                e.DragEffects = e.DataTransfer.Contains(TrackFormat) ? DragDropEffects.Move : DragDropEffects.None;
            }

            private void Drop(object? sender, DragEventArgs e)
            {
                if (_grid.DataContext is not PlaylistBaseViewModel viewModel || e.DataTransfer.TryGetValue(TrackFormat) is not { } track)
                {
                    return;
                }

                var playlist = viewModel.Playlist;
                var oldIndex = playlist.IndexOf(track);
                // Below the last track means to the end, above the first one to the top.
                var newIndex = playlist.Count - 1;
                if (TrackAt(e.Source) is { } target)
                {
                    newIndex = playlist.IndexOf(target);
                }
                else if (IsOnColumnHeaders(e.Source))
                {
                    newIndex = 0;
                }

                if (oldIndex < 0 || newIndex < 0 || oldIndex == newIndex)
                {
                    return;
                }

                viewModel.MoveTrack(track, newIndex);
                // Dragged around, it's not sorted by that column any more.
                ClearSortIndicators(_grid);
                _grid.SelectedItem = track;
            }
        }
    }
}
