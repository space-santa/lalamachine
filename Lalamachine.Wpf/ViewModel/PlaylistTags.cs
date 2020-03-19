using System.ComponentModel;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlaylistTags : LibLalaTags, INotifyPropertyChanged
    {
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

        public bool IsPlaying { get; set; }
    }
}
