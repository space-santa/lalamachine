using System.ComponentModel;
using System.Runtime.CompilerServices;
using LibLala.TagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlaylistTags : Tags, INotifyPropertyChanged
    {
        public PlaylistTags() : base() { }
        public PlaylistTags(Tags other) : base(other) { }
        public PlaylistTags(PlaylistTags other) : base(other) { IsPlaying = other.IsPlaying; }

        private bool isPlaying;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsPlaying { get => isPlaying; set { isPlaying = value; NotifyPropertyChanged(); } }
    }
}
