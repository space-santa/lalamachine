using Lalamachine.Wpf.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {
        public PlaylistControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaylistViewModel viewModel = (PlaylistViewModel)DataContext;
            viewModel.PlayTrackCommand.Execute(PlaylistView.SelectedItem);
        }
    }
}
