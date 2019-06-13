using Lalamachine.Wpf.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lalamachine.Wpf.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PlayerViewModel _player;
        private PlaylistViewModel _playlistViewModel;
        private ShuffleRepeatViewModel _shuffleRepeatViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _player = new PlayerViewModel();
            _playlistViewModel = new PlaylistViewModel();
            _shuffleRepeatViewModel = new ShuffleRepeatViewModel();
            _player.ManualLoadEvent += _playlistViewModel.ManualLoadHandler;
            _player.PlayNextTrackEvent += _playlistViewModel.PlayNextTrackHandler;
            _player.PlayLastTrackEvent += _playlistViewModel.PlayLastTrackHandler;
            _playlistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            Player.DataContext = _player;
            Player.ShuffleRepeatControl.DataContext = _shuffleRepeatViewModel;
            MainPlaylist.DataContext = _playlistViewModel;
        }
    }
}
