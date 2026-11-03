using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lalamachine.ViewModel
{
    public sealed class PlaylistChoice : BaseNotifyPropertyChanged
    {
        private readonly Action<bool> _changed;
        private bool _containsTrack;

        public PlaylistChoice(string name, bool containsTrack, Action<bool> changed)
        {
            Name = name;
            _containsTrack = containsTrack;
            _changed = changed;
        }

        public string Name { get; }

        public bool ContainsTrack
        {
            get => _containsTrack;
            set
            {
                if (value == _containsTrack) { return; }
                _containsTrack = value;
                NotifyPropertyChanged();
                _changed(value);
            }
        }
    }

    // All the playlists, and which of them have a track. Ticking one changes it right away.
    public sealed class TrackPlaylistChoices
    {
        public TrackPlaylistChoices(ObservableCollection<PlaylistChoice> choices, ICommand newPlaylistCommand)
        {
            Choices = choices;
            NewPlaylistCommand = newPlaylistCommand;
        }

        public ObservableCollection<PlaylistChoice> Choices { get; }
        public ICommand NewPlaylistCommand { get; }
    }
}
