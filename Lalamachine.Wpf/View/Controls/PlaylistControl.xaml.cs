using System.Windows.Controls;
using System.Windows.Input;
using Lalamachine.Wpf.ViewModel;

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
            var viewModel = (PlaylistViewModel)DataContext;
            viewModel.PlayTrackCommand.Execute(PlaylistView.SelectedItem);
        }
    }
}
