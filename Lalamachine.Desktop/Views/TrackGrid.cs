using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Lalamachine.ViewModel;
using LibLala.DomainPrimitives;

namespace Lalamachine.Desktop.Views
{
    // What the grids of tracks have in common.
    internal static class TrackGrid
    {
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

        private static bool IsOnColumnHeader(object? eventSource)
        {
            return (eventSource as Visual)?.FindAncestorOfType<DataGridColumnHeader>(includeSelf: true) is not null;
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

        public static void ClearSortIndicators(DataGrid grid)
        {
            foreach (var column in grid.Columns)
            {
                // The Tag is the column's name, the header also shows the sort direction.
                if (column.Tag is string columnName)
                {
                    column.Header = columnName;
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

            var ascendingHeader = $"{columnName} ▲";
            var direction = ascendingHeader.Equals(e.Column.Header) ? ListSortDirection.Descending : ListSortDirection.Ascending;

            ClearSortIndicators(grid);
            e.Column.Header = direction == ListSortDirection.Ascending ? ascendingHeader : $"{columnName} ▼";

            viewModel.SortCommand.Execute(new SortMessage(direction, new PlaylistColumn(columnName)));
        }

        private static void AddPlaylistChoices(DataGrid grid)
        {
            // The row that was right-clicked. It can be one of several selected ones, and the keyboard's menu key has none.
            PlaylistTags? clickedTrack = null;
            grid.AddHandler(
                InputElement.PointerPressedEvent,
                (sender, e) => clickedTrack = e.GetCurrentPoint(grid).Properties.IsRightButtonPressed ? TrackAt(e.Source) : null,
                RoutingStrategies.Tunnel);

            var menuItem = new MenuItem { Header = "Add to playlist..." };
            menuItem.Click += (sender, e) =>
            {
                if ((clickedTrack ?? grid.SelectedItem as PlaylistTags) is { } track)
                {
                    ShowPlaylistChoices(grid, track);
                }
            };
            grid.ContextMenu?.Items.Insert(0, menuItem);
        }

        private static void ShowPlaylistChoices(DataGrid grid, PlaylistTags track)
        {
            if (grid.DataContext is not PlaylistBaseViewModel viewModel)
            {
                return;
            }

            var flyout = new Flyout
            {
                Content = new TrackPlaylistsControl { DataContext = viewModel.TrackPlaylists.ChoicesFor(track) },
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
                data.Add(DataTransferItem.Create(TrackFormat, track));
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
                else if (IsOnColumnHeader(e.Source))
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
