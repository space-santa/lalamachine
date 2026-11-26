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

        // One for all grids, see ColumnSettings.
        private static readonly Lazy<ColumnSettings> Columns = new(() => new ColumnSettings());

        // How wide the columns start, by their names. Not as wide as what they show: that's only measured for the rows on
        // screen, a column would get wider as the list scrolls. The ones with a sort indicator have room for it.
        private static readonly Dictionary<string, double> ColumnWidths = new()
        {
            ["Playing"] = 48,
            ["Liked"] = 40,
            ["Title"] = 320,
            ["Length"] = 100,
            ["Key"] = 80,
            ["BPM"] = 85,
            ["Artist"] = 220,
            ["Genre"] = 170,
            ["Track"] = 90,
            ["Disc"] = 80,
            ["Comment"] = 200,
            ["Album"] = 240,
            ["Year"] = 80,
        };

        // A column with a button next to its name, the filter of the library's Key and BPM columns.
        private const double HeaderButtonWidth = 28;

        public static void Attach(DataGrid grid)
        {
            grid.Sorting += Sort;
            grid.AddHandler(InputElement.DoubleTappedEvent, Grid_DoubleTapped, handledEventsToo: true);
            SetWidths(grid, DefaultWidths(grid));
            FollowList(grid);
            AddDeselectOnClick(grid);
            AddPlaylistChoices(grid);
            AddKeyTooltips(grid);
        }

        private static Dictionary<string, double> DefaultWidths(DataGrid grid)
        {
            return grid.Columns
                .Where(x => ColumnWidths.ContainsKey(ColumnName(x)))
                .ToDictionary(ColumnName, x => ColumnWidths[ColumnName(x)] + (x.HeaderTemplate is null ? 0 : HeaderButtonWidth));
        }

        // As the user left them, also one dragged to the width it has from the start.
        private static Dictionary<string, double> Widths(DataGrid grid)
        {
            return grid.Columns.ToDictionary(ColumnName, x => x.Width.IsAbsolute ? x.Width.Value : x.ActualWidth);
        }

        // The ones of the columns that are named in it.
        private static void SetWidths(DataGrid grid, IReadOnlyDictionary<string, double> widths)
        {
            foreach (var column in grid.Columns)
            {
                if (widths.TryGetValue(ColumnName(column), out var width))
                {
                    column.Width = new DataGridLength(width);
                }
            }
        }

        private static bool AreSameWidths(Dictionary<string, double> widths, Dictionary<string, double> others)
        {
            return widths.Count == others.Count && widths.All(x => others.TryGetValue(x.Key, out var other) && Math.Abs(x.Value - other) < 0.5);
        }

        // The style of this class is what shows the tooltip.
        private static void AddKeyTooltips(DataGrid grid)
        {
            foreach (var column in grid.Columns.Where(x => "Key".Equals(x.Tag)))
            {
                column.CellStyleClasses.Add("key");
            }
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

        // What the grid shows comes from its list: what it's sorted by, and which track plays. The playlists' grid shows one
        // list after another.
        private static void FollowList(DataGrid grid)
        {
            PlaylistBaseViewModel? viewModel = null;
            PlaylistTags? playing = null;

            void ListChanged(object? sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(PlaylistBaseViewModel.CurrentSort):
                        ShowSort(grid);
                        break;
                    case nameof(PlaylistBaseViewModel.CurrentIndex):
                        var now = viewModel?.Playlist.FirstOrDefault(x => x.IsPlaying);
                        if (now is not null && now != playing)
                        {
                            SelectNextPlaying(grid, playing, now);
                        }
                        playing = now;
                        break;
                    // New objects for the same tracks, or the same ones in another order.
                    case nameof(PlaylistBaseViewModel.Playlist):
                        playing = viewModel?.Playlist.FirstOrDefault(x => x.IsPlaying);
                        break;
                }
            }

            void Follow()
            {
                if (viewModel is not null)
                {
                    viewModel.PropertyChanged -= ListChanged;
                }
                viewModel = grid.DataContext as PlaylistBaseViewModel;
                if (viewModel is not null)
                {
                    viewModel.PropertyChanged += ListChanged;
                }
                playing = viewModel?.Playlist.FirstOrDefault(x => x.IsPlaying);
                ShowSort(grid);
            }

            grid.DataContextChanged += (sender, e) => Follow();
            Follow();
        }

        // The selection goes on with the music, when it's on the track that played until now: double-clicked, or followed
        // there. Not when the user clicked another track, or nothing is selected. On screen when that track was.
        private static void SelectNextPlaying(DataGrid grid, PlaylistTags? before, PlaylistTags now)
        {
            if (before is null || grid.SelectedItems.Count != 1 || grid.SelectedItem != before)
            {
                return;
            }

            var wasOnScreen = grid.GetVisualDescendants().OfType<DataGridRow>().Any(x => x.IsVisible && x.DataContext == before);
            grid.SelectedItem = now;
            if (wasOnScreen)
            {
                grid.ScrollIntoView(now, null);
            }
        }

        // Clicking the one selected track again takes the selection away, when the click is let go: a press can start a drag
        // or a double click.
        private static void AddDeselectOnClick(DataGrid grid)
        {
            PlaylistTags? toDeselect = null;
            var pressedAt = new Point();

            grid.AddHandler(InputElement.PointerPressedEvent, (sender, e) =>
            {
                toDeselect = null;
                var point = e.GetCurrentPoint(grid);
                if (point.Properties.IsLeftButtonPressed
                    && e.KeyModifiers == KeyModifiers.None
                    && e.ClickCount == 1
                    && !IsOnButton(e.Source)
                    && TrackAt(e.Source) is { } track
                    && grid.SelectedItems.Count == 1
                    && grid.SelectedItem == track)
                {
                    toDeselect = track;
                    pressedAt = point.Position;
                }
            }, RoutingStrategies.Tunnel);

            grid.AddHandler(InputElement.PointerReleasedEvent, (sender, e) =>
            {
                var moved = e.GetPosition(grid) - pressedAt;
                if (toDeselect is not null && grid.SelectedItem == toDeselect && TrackAt(e.Source) == toDeselect && Math.Abs(moved.X) < 4 && Math.Abs(moved.Y) < 4)
                {
                    grid.SelectedItem = null;
                }
                toDeselect = null;
            }, RoutingStrategies.Tunnel, handledEventsToo: true);
        }

        // Shows the columns in the order and as wide as the user dragged them to the last time. Right-clicking the headers
        // has the default order or widths back. Every grid has its own, under its name. After Attach, which sets the
        // widths they start with.
        public static void RememberColumns(DataGrid grid, string gridName)
        {
            // The one in the XAML.
            var defaultOrder = grid.Columns.Select(ColumnName).ToList();
            Arrange(grid, Columns.Value.OrderOf(gridName, defaultOrder));
            // Only after the user dragged a column, not when the order is set here.
            grid.ColumnReordered += (sender, e) => Columns.Value.SetOrderOf(gridName, ColumnOrder(grid), defaultOrder);

            var defaultWidths = DefaultWidths(grid);
            SetWidths(grid, Columns.Value.WidthsOf(gridName));
            // A column is made wider or narrower by dragging the edge of its header, which ends with letting go of it.
            // The grid has no event for that.
            var savedWidths = Widths(grid);
            grid.AddHandler(InputElement.PointerReleasedEvent, (sender, e) =>
            {
                var widths = Widths(grid);
                if (!AreSameWidths(widths, savedWidths))
                {
                    savedWidths = widths;
                    Columns.Value.SetWidthsOf(gridName, widths, defaultWidths);
                }
            }, RoutingStrategies.Bubble, handledEventsToo: true);

            var restoreOrderItem = new MenuItem { Header = "Restore default column order" };
            restoreOrderItem.Click += (sender, e) =>
            {
                Arrange(grid, defaultOrder);
                Columns.Value.ResetOrderOf(gridName);
            };
            var restoreWidthsItem = new MenuItem { Header = "Restore default column widths" };
            restoreWidthsItem.Click += (sender, e) =>
            {
                SetWidths(grid, defaultWidths);
                savedWidths = Widths(grid);
                Columns.Value.ResetWidthsOf(gridName);
            };
            var menu = new ContextMenu { Items = { restoreOrderItem, restoreWidthsItem } };
            menu.Opening += (sender, e) =>
            {
                restoreOrderItem.IsEnabled = !ColumnOrder(grid).SequenceEqual(defaultOrder);
                restoreWidthsItem.IsEnabled = !AreSameWidths(Widths(grid), defaultWidths);
            };
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

        // The direction in the header of the column that the list is sorted by, none in the others.
        private static void ShowSort(DataGrid grid)
        {
            var sort = (grid.DataContext as PlaylistBaseViewModel)?.CurrentSort;
            foreach (var column in grid.Columns)
            {
                var name = ColumnName(column);
                if (sort is not null && sort.Column.Name == name)
                {
                    column.Header = name + (sort.Direction == ListSortDirection.Ascending ? AscendingIndicator : DescendingIndicator);
                }
                // Only the headers with a direction: the name of the ♥ column isn't its header, it has none.
                else if (column.Header is string header && (header.EndsWith(AscendingIndicator, StringComparison.Ordinal) || header.EndsWith(DescendingIndicator, StringComparison.Ordinal)))
                {
                    column.Header = name;
                }
            }
        }

        // Sorts the playlist itself, not just the grid's view of it, so that next and previous follow the order on screen.
        // Up, down, and then back to the order from before.
        private static void Sort(object? sender, DataGridColumnEventArgs e)
        {
            e.Handled = true;
            if (sender is not DataGrid { DataContext: PlaylistBaseViewModel viewModel } || e.Column.Tag is not string columnName)
            {
                return;
            }

            var sort = viewModel.CurrentSort;
            if (sort is null || sort.Column.Name != columnName)
            {
                viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn(columnName)));
            }
            else if (sort.Direction == ListSortDirection.Ascending)
            {
                viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Descending, new PlaylistColumn(columnName)));
            }
            else
            {
                viewModel.UnsortCommand.Execute(null);
            }
        }

        private static void AddPlaylistChoices(DataGrid grid)
        {
            var menuItem = new MenuItem { Header = "Add to playlist..." };
            menuItem.Click += (sender, e) => ShowPlaylistChoices(grid);
            if (grid.ContextMenu is { } contextMenu)
            {
                contextMenu.Items.Insert(0, menuItem);
                contextMenu.Opening += (sender, e) => menuItem.IsEnabled = SelectedTracks(grid).Count > 0;
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

                // Dragged around, it's not sorted by that column any more: the list says so.
                viewModel.MoveTrack(track, newIndex);
                _grid.SelectedItem = track;
            }
        }
    }

    // The row of a track that plays has the class playing, for its own colour. A style binds this to the track.
    internal sealed class TrackRow
    {
        public static readonly AttachedProperty<bool> IsPlayingProperty = AvaloniaProperty.RegisterAttached<TrackRow, DataGridRow, bool>("IsPlaying");

        static TrackRow()
        {
            IsPlayingProperty.Changed.AddClassHandler<DataGridRow>((row, e) => row.Classes.Set("playing", e.NewValue is true));
        }

        private TrackRow() { }

        public static bool GetIsPlaying(DataGridRow row) => row?.GetValue(IsPlayingProperty) ?? false;

        public static void SetIsPlaying(DataGridRow row, bool value) => row?.SetValue(IsPlayingProperty, value);
    }
}
