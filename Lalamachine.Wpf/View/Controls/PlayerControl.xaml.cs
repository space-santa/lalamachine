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

        private void PlayerButtonsControl_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            var source = (PlayerButtonsControl)e.Source;
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Music | *.mp3; *.m4a";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                var path = dlg.FileName;
                _player.Open(path);
            }
        }
    }
}
