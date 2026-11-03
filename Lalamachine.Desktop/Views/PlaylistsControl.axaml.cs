using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lalamachine.Desktop.Views
{
    public partial class PlaylistsControl : UserControl
    {
        public PlaylistsControl()
        {
            InitializeComponent();
            TrackGrid.Attach(PlaylistGrid);
            TrackGrid.EnableReorder(PlaylistGrid);
        }

        private void DiscardButton_Click(object? sender, RoutedEventArgs e)
        {
            TrackGrid.ClearSortIndicators(PlaylistGrid);
        }
    }
}
