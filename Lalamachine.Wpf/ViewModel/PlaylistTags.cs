using System.ComponentModel;
using System.Runtime.CompilerServices;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlaylistTags : LibLalaTags, INotifyPropertyChanged
    {
        public PlaylistTags(string title, string path) : base(title, path) { }
        public PlaylistTags(LibLalaTags other) : base(other) { }
        public PlaylistTags(PlaylistTags other) : base(other)
        {
            if (other is null)
            {
                return;
            }

            IsPlaying = other.IsPlaying;
        }

        private bool _isPlaying;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsPlaying { get => _isPlaying; set { _isPlaying = value; NotifyPropertyChanged(); } }
    }
}
