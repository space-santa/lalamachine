using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class GenreCombinationsViewModelTest
    {
        private IDialogService _dialogs = null!;
        private InMemorySettingsFile _file = null!;
        private List<string> _selectedGenres = null!;

        [SetUp]
        public void SetUp()
        {
            _dialogs = Substitute.For<IDialogService>();
            _file = new InMemorySettingsFile();
            _selectedGenres = [];
        }

        private GenreCombinationsViewModel ViewModel()
        {
            return new GenreCombinationsViewModel(new GenreCombinationSettings(_file), _dialogs, () => _selectedGenres);
        }

        private void NameTheNextCombination(string? name)
        {
            _dialogs.AskForTextAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(name));
        }

        private void AnswerConfirm(bool answer)
        {
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(answer));
        }

        private static IEnumerable<(string, string)> Saved(GenreCombinationsViewModel viewModel)
        {
            return viewModel.Combinations.Select(x => (x.Name, x.GenresString));
        }

        [Test]
        public void Save_TheSelectedGenres_UnderTheGivenName_SuggestingTheirNames()
        {
            _selectedGenres = ["House", "Techno"];
            NameTheNextCombination("  Club ");
            var viewModel = ViewModel();

            viewModel.SaveCommand.Execute(null);

            _dialogs.Received(1).AskForTextAsync(Arg.Any<string>(), "House + Techno");
            Assert.That(Saved(viewModel), Is.EqualTo(new[] { ("Club", "House, Techno") }));
            Assert.That(viewModel.HasCombinations, Is.True);
        }

        [Test]
        public void Saved_AreThereNextTime_ByName()
        {
            _selectedGenres = ["Jazz"];
            var viewModel = ViewModel();
            NameTheNextCombination("Zzz");
            viewModel.SaveCommand.Execute(null);
            NameTheNextCombination("aaa");
            viewModel.SaveCommand.Execute(null);

            var next = ViewModel();

            Assert.That(next.Combinations.Select(x => x.Name), Is.EqualTo(new[] { "aaa", "Zzz" }));
            Assert.That(next.Combinations[0].Genres, Is.EqualTo(new[] { "Jazz" }));
        }

        [Test]
        public void Save_Cancelled_SavesNothing()
        {
            _selectedGenres = ["Jazz"];
            NameTheNextCombination(null);
            var viewModel = ViewModel();

            viewModel.SaveCommand.Execute(null);

            Assert.That(viewModel.Combinations, Is.Empty);
            Assert.That(ViewModel().Combinations, Is.Empty);
        }

        [Test]
        public void Save_NoGenreSelected_DoesNotEvenAsk()
        {
            var viewModel = ViewModel();

            viewModel.SaveCommand.Execute(null);

            _dialogs.DidNotReceive().AskForTextAsync(Arg.Any<string>(), Arg.Any<string>());
            Assert.That(viewModel.CanSave, Is.False);
        }

        [TestCase(true, "Rock")]
        [TestCase(false, "Jazz")]
        public void Save_NameTaken_ReplacesItWhenTheUserSaysSo(bool answer, string genres)
        {
            _selectedGenres = ["Jazz"];
            NameTheNextCombination("Mine");
            var viewModel = ViewModel();
            viewModel.SaveCommand.Execute(null);
            _selectedGenres = ["Rock"];
            NameTheNextCombination("mine");
            AnswerConfirm(answer);

            viewModel.SaveCommand.Execute(null);

            Assert.That(viewModel.Combinations.Select(x => x.GenresString), Is.EqualTo(new[] { genres }));
            Assert.That(ViewModel().Combinations.Select(x => x.GenresString), Is.EqualTo(new[] { genres }));
        }

        [TestCase(true, new string[0])]
        [TestCase(false, new[] { "Mine" })]
        public void Delete_WhenTheUserSaysSo(bool answer, string[] left)
        {
            _selectedGenres = ["Jazz"];
            NameTheNextCombination("Mine");
            var viewModel = ViewModel();
            viewModel.SaveCommand.Execute(null);
            AnswerConfirm(answer);

            viewModel.DeleteCommand.Execute(viewModel.Combinations[0]);

            Assert.That(viewModel.Combinations.Select(x => x.Name), Is.EqualTo(left));
            Assert.That(ViewModel().Combinations.Select(x => x.Name), Is.EqualTo(left));
            Assert.That(viewModel.HasCombinations, Is.EqualTo(!answer));
        }

        [Test]
        public void SettingsThatCantBeRead_AreNoCombinations()
        {
            var file = new InMemorySettingsFile();
            file.Save(new Dictionary<string, string> { ["Combinations"] = "not json" });

            var viewModel = new GenreCombinationsViewModel(new GenreCombinationSettings(file), _dialogs, () => _selectedGenres);

            Assert.That(viewModel.Combinations, Is.Empty);
        }

        [Test]
        public void SettingsWithNulls_HaveTheCombinationsThatAreThere()
        {
            var file = new InMemorySettingsFile();
            file.Save(new Dictionary<string, string>
            {
                ["Combinations"] = """[null, {"Name": "Mine", "Genres": [null, "Rock", " "]}, {"Name": null, "Genres": ["Jazz"]}, {"Name": "Empty", "Genres": null}]""",
            });

            var viewModel = new GenreCombinationsViewModel(new GenreCombinationSettings(file), _dialogs, () => _selectedGenres);

            Assert.That(viewModel.Combinations.Select(x => (x.Name, x.GenresString)), Is.EqualTo(new[] { ("Mine", "Rock") }));
        }

        [Test]
        public void GenreSelectionChanged_TellsTheViewWhetherItCanSave()
        {
            var viewModel = ViewModel();
            var notifications = new List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            _selectedGenres = ["Jazz"];
            viewModel.GenreSelectionChanged();

            Assert.That(viewModel.CanSave, Is.True);
            Assert.That(notifications, Does.Contain(nameof(viewModel.CanSave)));
        }
    }
}
