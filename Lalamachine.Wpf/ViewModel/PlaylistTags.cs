using System.ComponentModel;
using System.Runtime.CompilerServices;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlaylistTags : LibLalaTags, INotifyPropertyChanged
    {
        private bool _isPlaying;

        public PlaylistTags(LibLalaTags other) : base(other) { }
        public PlaylistTags(PlaylistTags other) : base(other)
        {
            if (other is null)
            {
                return;
            }

            IsPlaying = other.IsPlaying;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsPlaying 
        { 
            get => _isPlaying; 
            set 
            {
                _isPlaying = value; 
                NotifyPropertyChanged(); 
            } 
        }
    }
}
