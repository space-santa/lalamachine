using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    public class ColumnOrderSettingsTest
    {
        private static readonly string[] DefaultOrder = ["Title", "Length", "Key", "BPM"];

        private ColumnOrderSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = new ColumnOrderSettings(new InMemorySettingsFile());
        }

        [Test]
        public void OrderOf_IsTheDefault_WhenNothingIsSaved()
        {
            Assert.That(_settings.OrderOf("Library", DefaultOrder), Is.EqualTo(DefaultOrder));
        }

        [Test]
        public void OrderOf_IsTheSavedOrder()
        {
            _settings.SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            Assert.That(_settings.OrderOf("Library", DefaultOrder), Is.EqualTo(new[] { "Key", "BPM", "Title", "Length" }));
        }

        [Test]
        public void OrderOf_IsTheOneOfTheGrid()
        {
            _settings.SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            Assert.That(_settings.OrderOf("Queue", DefaultOrder), Is.EqualTo(DefaultOrder));
        }

        [Test]
        public void OrderOf_IsKept_AfterTheAppStartsAgain()
        {
            var file = new InMemorySettingsFile();
            new ColumnOrderSettings(file).SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            Assert.That(new ColumnOrderSettings(file).OrderOf("Library", DefaultOrder), Is.EqualTo(new[] { "Key", "BPM", "Title", "Length" }));
        }

        [Test]
        public void OrderOf_IsTheDefault_AfterReset()
        {
            _settings.SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            _settings.ResetOrderOf("Library");

            Assert.That(_settings.OrderOf("Library", DefaultOrder), Is.EqualTo(DefaultOrder));
        }

        [Test]
        public void OrderOf_FollowsANewDefault_WhenTheDefaultWasSaved()
        {
            // E.g. a column dragged away and back.
            _settings.SetOrderOf("Library", DefaultOrder, DefaultOrder);

            string[] newDefaultOrder = ["Title", "Comment", "Length", "Key", "BPM"];
            Assert.That(_settings.OrderOf("Library", newDefaultOrder), Is.EqualTo(newDefaultOrder));
        }

        [Test]
        public void OrderOf_IsTheDefault_AfterTheDefaultWasSavedOverAnother()
        {
            _settings.SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            _settings.SetOrderOf("Library", DefaultOrder, DefaultOrder);

            string[] newDefaultOrder = ["Length", "Title", "Key", "BPM"];
            Assert.That(_settings.OrderOf("Library", newDefaultOrder), Is.EqualTo(newDefaultOrder));
        }

        [Test]
        public void OrderOf_PutsANewColumnAtTheEnd()
        {
            _settings.SetOrderOf("Library", ["Key", "Title", "Length"], DefaultOrder);

            Assert.That(_settings.OrderOf("Library", DefaultOrder), Is.EqualTo(new[] { "Key", "Title", "Length", "BPM" }));
        }

        [Test]
        public void OrderOf_LeavesOutAColumnThatIsGone()
        {
            _settings.SetOrderOf("Library", ["Key", "Comment", "BPM", "Title", "Length"], DefaultOrder);

            Assert.That(_settings.OrderOf("Library", DefaultOrder), Is.EqualTo(new[] { "Key", "BPM", "Title", "Length" }));
        }
    }
}
