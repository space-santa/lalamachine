using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Settings;

namespace Lalamachine.ViewModel
{
    // Genres selected together and saved under a name, to select them again in one go.
    public sealed record GenreCombination(string Name, IReadOnlyList<string> Genres)
    {
        [JsonIgnore]
        public string GenresString => string.Join(", ", Genres);
    }

    public class GenreCombinationSettings : AppSettingsBase
    {
        public GenreCombinationSettings() : this(new SettingsFile("Lalamachine.Desktop", "GenreCombinationSettings")) { }

        public GenreCombinationSettings(SettingsFile file) : base(file) { }

        // Empty when none are saved, or what's saved can't be read.
        public IReadOnlyList<GenreCombination> Combinations
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<List<GenreCombination>>(GetString("[]")) ?? [];
                }
                catch (JsonException)
                {
                    return [];
                }
            }
            set => Set(JsonSerializer.Serialize(value));
        }
    }

    // The saved genre combinations of the library. Selecting one is up to the view, it's what the genre list has selected.
    public sealed class GenreCombinationsViewModel : BaseNotifyPropertyChanged
    {
        private readonly GenreCombinationSettings _settings;
        private readonly IDialogService _dialogs;
        private readonly Func<IReadOnlyList<string>> _selectedGenres;

        public GenreCombinationsViewModel(GenreCombinationSettings settings, IDialogService dialogs, Func<IReadOnlyList<string>> selectedGenres)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _settings = settings;
            _dialogs = dialogs;
            _selectedGenres = selectedGenres;
            // Also the ones of a file that someone edited, with two of the same name or with no genres.
            Combinations = new ObservableCollection<GenreCombination>(
                settings.Combinations
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Genres is { Count: > 0 })
                    .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase));
            _saveCommand = new DelegateCommand(OnSave);
            _deleteCommand = new DelegateCommand(OnDelete);
        }

        // By name.
        public ObservableCollection<GenreCombination> Combinations { get; }

        public bool HasCombinations => Combinations.Count > 0;

        public bool CanSave => _selectedGenres().Count > 0;

        // The library tells it when other genres are selected.
        public void GenreSelectionChanged()
        {
            NotifyPropertyChanged(nameof(CanSave));
        }

        // The genres selected now, under a name the user gives them. The genres' names are what it's called, unless the
        // user names it something else.
        private readonly DelegateCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand;
        private async void OnSave(object? obj)
        {
            var genres = _selectedGenres().ToList();
            if (genres.Count == 0)
            {
                return;
            }

            var name = (await _dialogs.AskForTextAsync("Name of the genre combination:", string.Join(" + ", genres)).ConfigureAwait(true))?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var taken = Combinations.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (taken is not null)
            {
                if (!await _dialogs.ConfirmAsync($"There already is a genre combination called {taken.Name}. Replace it?").ConfigureAwait(true))
                {
                    return;
                }
                Combinations.Remove(taken);
            }

            var combination = new GenreCombination(name, genres);
            Combinations.Insert(Combinations.Count(x => StringComparer.CurrentCultureIgnoreCase.Compare(x.Name, name) < 0), combination);
            Save();
        }

        private readonly DelegateCommand _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand;
        private async void OnDelete(object? obj)
        {
            if (obj is not GenreCombination combination || !Combinations.Contains(combination))
            {
                return;
            }

            if (!await _dialogs.ConfirmAsync($"Delete the genre combination {combination.Name}?").ConfigureAwait(true))
            {
                return;
            }

            Combinations.Remove(combination);
            Save();
        }

        private void Save()
        {
            _settings.Combinations = Combinations.ToList();
            NotifyPropertyChanged(nameof(HasCombinations));
        }
    }
}
