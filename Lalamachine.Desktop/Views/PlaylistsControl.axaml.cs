using Avalonia.Controls;

namespace Lalamachine.Desktop.Views
{
    public partial class PlaylistsControl : UserControl
    {
        public PlaylistsControl()
        {
            InitializeComponent();
            TrackGrid.Attach(PlaylistGrid);
            TrackGrid.EnableReorder(PlaylistGrid);
            TrackGrid.RememberColumns(PlaylistGrid, "Playlists");
        }
    }
}
