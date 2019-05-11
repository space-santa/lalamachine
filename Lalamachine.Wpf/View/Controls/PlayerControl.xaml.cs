using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using Lalamachine.Wpf.ViewModel;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        private PlayerViewModel _player;
        public PlayerControl()
        {
            _player = new PlayerViewModel();
            DataContext = _player;
            InitializeComponent();
        }
    }
}
