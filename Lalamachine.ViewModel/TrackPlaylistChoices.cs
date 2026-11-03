using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lalamachine.ViewModel
{
    public sealed class PlaylistChoice : BaseNotifyPropertyChanged
    {
        private readonly Action<bool> _changed;
        private bool _containsTracks;

        public PlaylistChoice(string name, bool containsTracks, Action<bool> changed)
        {
            Name = name;
            _containsTracks = containsTracks;
            _changed = changed;
        }

        public string Name { get; }

        // True only if it has all of them. Ticking adds the ones it doesn't have, unticking removes them all.
        public bool ContainsTracks
        {
            get => _containsTracks;
            set
            {
                if (value == _containsTracks) { return; }
                // First, if that fails it isn't ticked.
                _changed(value);
                _containsTracks = value;
                NotifyPropertyChanged();
            }
        }
    }

    // All the playlists, and which of them have the tracks. Ticking one changes it right away.
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
