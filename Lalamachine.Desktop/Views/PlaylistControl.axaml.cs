using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Views
{
    public partial class PlaylistControl : UserControl
    {
        // Tracks are only dragged within the playlist, to change their order.
        private static readonly DataFormat<PlaylistTags> TrackFormat = DataFormat.CreateInProcessFormat<PlaylistTags>("lalamachine-playlist-track");

        private PointerPressedEventArgs? _dragStart;

        public PlaylistControl()
        {
            InitializeComponent();
            TrackGrid.Attach(PlaylistGrid);

            // Tunnel, so that the grid's own selection handling doesn't hide these.
            PlaylistGrid.AddHandler(PointerPressedEvent, PlaylistGrid_PointerPressed, RoutingStrategies.Tunnel);
            PlaylistGrid.AddHandler(PointerMovedEvent, PlaylistGrid_PointerMoved, RoutingStrategies.Tunnel);
            PlaylistGrid.AddHandler(PointerReleasedEvent, PlaylistGrid_PointerReleased, RoutingStrategies.Tunnel);
            PlaylistGrid.AddHandler(DragDrop.DragOverEvent, PlaylistGrid_DragOver);
            PlaylistGrid.AddHandler(DragDrop.DropEvent, PlaylistGrid_Drop);
        }

        private void PlaylistGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var isLeftButton = e.GetCurrentPoint(PlaylistGrid).Properties.IsLeftButtonPressed;
            _dragStart = isLeftButton && TrackGrid.TrackAt(e.Source) is not null ? e : null;
        }

        private void PlaylistGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _dragStart = null;
        }

        private async void PlaylistGrid_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_dragStart is null || TrackGrid.TrackAt(_dragStart.Source) is not { } track)
            {
                return;
            }

            // A click wobbles a little, a drag moves further.
            var distance = e.GetPosition(PlaylistGrid) - _dragStart.GetPosition(PlaylistGrid);
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

        private void PlaylistGrid_DragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = e.DataTransfer.Contains(TrackFormat) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void PlaylistGrid_Drop(object? sender, DragEventArgs e)
        {
            if (DataContext is not PlaylistViewModel viewModel || e.DataTransfer.TryGetValue(TrackFormat) is not { } track)
            {
                return;
            }

            var playlist = viewModel.Playlist;
            var oldIndex = playlist.IndexOf(track);
            // Below the last track means to the end.
            var newIndex = TrackGrid.TrackAt(e.Source) is { } target ? playlist.IndexOf(target) : playlist.Count - 1;
            if (oldIndex < 0 || newIndex < 0 || oldIndex == newIndex)
            {
                return;
            }

            playlist.Move(oldIndex, newIndex);
            PlaylistGrid.SelectedItem = track;
        }
    }
}
