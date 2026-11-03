using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
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

        [Test]
        public void PrivateSettingsLoadAgain()
        {
            var settingsFile = new PrivateSettingsFile("Joe", "Bob", _settingsDir);
            settingsFile.Save(new Dictionary<string, string> { ["Volume"] = "37.5" });
            settingsFile.Save(new Dictionary<string, string> { ["Volume"] = "12.5" });

            Assert.That(settingsFile.Load()["Volume"], Is.EqualTo("12.5"));
            Assert.That(Directory.GetFiles(_settingsDir), Is.EqualTo(new[] { Path.Combine(_settingsDir, "Bob.json") }));
        }

        [Test]
        public void SavedSecretLoadsAgainFromAnotherInstance()
        {
            var settings = new TestSettings(new PrivateSettingsFile("Joe", "Bob", _settingsDir))
            {
                TestSecret = "Bob's key"
            };

            Assert.That(new TestSettings(new PrivateSettingsFile("Joe", "Bob", _settingsDir)).TestSecret, Is.EqualTo("Bob's key"));
            Assert.That(settings.TestSecret, Is.EqualTo("Bob's key"));
        }

        [Test]
        [Platform("Win")]
        public void SavedSecretIsNotReadableInTheFileOnWindows()
        {
            _ = new TestSettings(new PrivateSettingsFile("Joe", "Bob", _settingsDir))
            {
                TestSecret = "Bob's key"
            };

            Assert.That(File.ReadAllText(Path.Combine(_settingsDir, "Bob.json")), Does.Not.Contain("Bob"));
        }

        [Test]
        [UnsupportedOSPlatform("windows")]
        public void OnlyTheUserCanReadAPrivateFile()
        {
            var settingsFile = new PrivateSettingsFile("Joe", "Bob", _settingsDir);
            settingsFile.Save(new Dictionary<string, string> { ["Volume"] = "37.5" });
            // Saving replaces the file, the new one has to be private as well.
            settingsFile.Save(new Dictionary<string, string> { ["Volume"] = "12.5" });

            Assert.That(File.GetUnixFileMode(Path.Combine(_settingsDir, "Bob.json")), Is.EqualTo(UnixFileMode.UserRead | UnixFileMode.UserWrite));
        }
    }
}
