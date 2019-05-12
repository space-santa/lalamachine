using System.Windows.Controls;
using Lalamachine.Wpf.ViewModel;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        private readonly PlayerViewModel _player;
        public PlayerControl()
        {
            _player = new PlayerViewModel();
            DataContext = _player;
            InitializeComponent();
        }
    }
}
