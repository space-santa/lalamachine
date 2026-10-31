using Avalonia.Controls;

namespace Lalamachine.Desktop.Views
{
    public partial class LibraryPlaylistControl : UserControl
    {
        public LibraryPlaylistControl()
        {
            InitializeComponent();
            TrackGrid.Attach(LibraryPlaylistGrid);
        }
    }
}
