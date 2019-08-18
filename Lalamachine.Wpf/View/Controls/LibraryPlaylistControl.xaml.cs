using System.Windows.Controls;
using System.Windows.Input;
using Lalamachine.Wpf.ViewModel;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for LibraryPlaylistControl.xaml
    /// </summary>
    public partial class LibraryPlaylistControl : UserControl
    {
        public LibraryPlaylistControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (PlaylistViewModel)DataContext;
            viewModel.PlayTrackCommand.Execute(LibraryPlaylistView.SelectedItem);
        }
    }
}
