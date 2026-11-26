using System.Collections.Generic;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    public class ColumnSettingsTest
    {
        private static readonly string[] DefaultOrder = ["Title", "Length", "Key", "BPM"];

        private ColumnSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = new ColumnSettings(new InMemorySettingsFile());
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
            new ColumnSettings(file).SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            Assert.That(new ColumnSettings(file).OrderOf("Library", DefaultOrder), Is.EqualTo(new[] { "Key", "BPM", "Title", "Length" }));
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

        private static readonly Dictionary<string, double> DefaultWidths = new() { ["Title"] = 320, ["Length"] = 100, ["BPM"] = 113 };

        [Test]
        public void WidthsOf_IsNothing_WhenNothingIsSaved()
        {
            Assert.That(_settings.WidthsOf("Library"), Is.Empty);
        }

        [Test]
        public void WidthsOf_AreTheChangedOnes_InWholePixels_AfterTheAppStartsAgain()
        {
            var file = new InMemorySettingsFile();

            new ColumnSettings(file).SetWidthsOf("Library", new Dictionary<string, double> { ["Title"] = 401.6, ["Length"] = 100.2, ["BPM"] = 90 }, DefaultWidths);

            Assert.That(new ColumnSettings(file).WidthsOf("Library"), Is.EquivalentTo(new Dictionary<string, double> { ["Title"] = 402, ["BPM"] = 90 }));
        }

        [Test]
        public void WidthsOf_IsTheOneOfTheGrid_AndLeavesItsOrderAlone()
        {
            _settings.SetOrderOf("Library", ["Key", "BPM", "Title", "Length"], DefaultOrder);

            _settings.SetWidthsOf("Library", new Dictionary<string, double> { ["Title"] = 400 }, DefaultWidths);

            Assert.That(_settings.WidthsOf("Queue"), Is.Empty);
            Assert.That(_settings.OrderOf("Library", DefaultOrder), Is.EqualTo(new[] { "Key", "BPM", "Title", "Length" }));
        }

        [Test]
        public void WidthsOf_IsNothing_AfterReset_OrWhenTheyAreBackToTheDefault()
        {
            _settings.SetWidthsOf("Library", new Dictionary<string, double> { ["Title"] = 400 }, DefaultWidths);
            _settings.SetWidthsOf("Queue", new Dictionary<string, double> { ["Title"] = 400 }, DefaultWidths);

            _settings.ResetWidthsOf("Library");
            _settings.SetWidthsOf("Queue", new Dictionary<string, double> { ["Title"] = 320 }, DefaultWidths);

            Assert.That(_settings.WidthsOf("Library"), Is.Empty);
            Assert.That(_settings.WidthsOf("Queue"), Is.Empty);
        }

        [Test]
        public void WidthsOf_SkipsWhatCantBeRead()
        {
            var file = new InMemorySettingsFile();
            file.Save(new Dictionary<string, string> { ["Library.Widths"] = "Title=400,Length,BPM=x,Key=-5,=7,Year=80" });

            Assert.That(new ColumnSettings(file).WidthsOf("Library"), Is.EquivalentTo(new Dictionary<string, double> { ["Title"] = 400, ["Year"] = 80 }));
        }
    }
}
