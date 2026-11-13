using System.ComponentModel;
using System.Runtime.CompilerServices;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    public class PlaylistTags : LibLalaTags, INotifyPropertyChanged
    {
        private bool _isPlaying;
        private bool _isLiked;

        public PlaylistTags(LibLalaTags other) : base(other) { }
        public PlaylistTags(PlaylistTags other) : base(other)
        {
            if (other is null)
            {
                return;
            }

            IsPlaying = other.IsPlaying;
            IsLiked = other.IsLiked;
            IsOutsideLibrary = other.IsOutsideLibrary;
        }

        // Loaded by hand, also if the library happens to have the file. It can play in the queue but not be in a playlist.
        public bool IsOutsideLibrary { get; init; }

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

        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                _isLiked = value;
                NotifyPropertyChanged();
            }
        }

        // What the analysis found in a track whose tags have no key, or no tempo. Null for the one that the tags have.
        public void ShowDetected(string? initialKey, uint? beatsPerMinute)
        {
            if (initialKey is not null && initialKey != InitialKey)
            {
                SetInitialKey(initialKey);
                NotifyPropertyChanged(nameof(InitialKey));
                NotifyPropertyChanged(nameof(InitialKeyNaturalNotation));
            }
            if (beatsPerMinute is { } value && value != BeatsPerMinute)
            {
                SetBeatsPerMinute(value);
                NotifyPropertyChanged(nameof(BeatsPerMinute));
                NotifyPropertyChanged(nameof(BeatsPerMinuteString));
            }
        }
    }
}
