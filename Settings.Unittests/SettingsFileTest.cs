using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Settings.Unittests
{
    public class SettingsFileTest
    {
        private string _settingsDir = "";

        [SetUp]
        public void SetUp()
        {
            _settingsDir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_settingsDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_settingsDir, recursive: true);
        }

        [Test]
        public void CorruptFileLoadsAsEmpty()
        {
            File.WriteAllText(Path.Combine(_settingsDir, "Bob.json"), "{\"Volume\": \"5");
            var settingsFile = new SettingsFile("Joe", "Bob", _settingsDir);

            Assert.That(settingsFile.Load(), Is.Empty);
        }

        [Test]
        public void MissingFileLoadsAsEmpty()
        {
            var settingsFile = new SettingsFile("Joe", "Bob", _settingsDir);

            Assert.That(settingsFile.Load(), Is.Empty);
        }

        [Test]
        public void SavedSettingsLoadAgain()
        {
            var settingsFile = new SettingsFile("Joe", "Bob", _settingsDir);
            settingsFile.Save(new Dictionary<string, string> { ["Volume"] = "37.5" });

            Assert.That(settingsFile.Load()["Volume"], Is.EqualTo("37.5"));
            Assert.That(Directory.GetFiles(_settingsDir), Is.EqualTo(new[] { Path.Combine(_settingsDir, "Bob.json") }));
        }
    }
}
