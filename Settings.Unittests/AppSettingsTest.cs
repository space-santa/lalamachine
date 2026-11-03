using System.Collections.Generic;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace Settings.Unittests
{
    internal sealed class TestSettings : AppSettingsBase
    {
        public TestSettings(SettingsFile settingsFile) : base(settingsFile) { }

        public int TestInt { get => GetInt(); set => Set(value); }
        public bool TestBool { get => GetBool(); set => Set(value); }
        public double TestDouble { get => GetDouble(); set => Set(value); }
        public string TestString { get => GetString(); set => Set(value); }
        public string TestSecret { get => GetSecret(); set => SetSecret(value); }
    }

    public class AppSettingsTest
    {
        [Test]
        public void TestIntShouldSave()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestInt = 5
            };
            settingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestInt"] == "5"));
        }

        [Test]
        public void TestBoolShouldSave()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestBool = true
            };
            settingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestBool"] == true.ToString(LibLala.Constants.CULTURE)));
        }

        [Test]
        public void TestDoubleShouldSave()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestDouble = 5.5
            };
            settingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestDouble"] == "5.5"));
        }

        [Test]
        public void TestStringShouldSave()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestString = "Bob goes to town."
            };
            settingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestString"] == "Bob goes to town."));
        }

        [Test]
        public void UnchangedValueIsNotSavedAgain()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestDouble = 5.5
            };
            settings.TestDouble = 5.5;
            settingsFile.Received(1).Save(Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void UnreadableValueFallsBackToDefault()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string> { ["TestInt"] = "lots", ["TestBool"] = "maybe", ["TestDouble"] = "" });
            var settings = new TestSettings(settingsFile);
            Assert.That(settings.TestInt, Is.EqualTo(0));
            Assert.That(settings.TestBool, Is.False);
            Assert.That(settings.TestDouble, Is.EqualTo(0.0));
        }

        [Test]
        public void SecretCanBeReadBack()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestSecret = "Bob's key"
            };
            Assert.That(settings.TestSecret, Is.EqualTo("Bob's key"));
        }

        [Test]
        public void MissingSecretIsEmpty()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile);
            Assert.That(settings.TestSecret, Is.Empty);
        }

        [Test]
        public void UnchangedSecretIsNotSavedAgain()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestSecret = "Bob's key"
            };
            settings.TestSecret = "Bob's key";
            settingsFile.Received(1).Save(Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SecretCanBeRemoved()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestSecret = "Bob's key"
            };
            settings.TestSecret = "";
            Assert.That(settings.TestSecret, Is.Empty);
            settingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestSecret"] == ""));
        }

        [Test]
        [Platform("Win")]
        public void SecretIsNotStoredAsItIsOnWindows()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(settingsFile)
            {
                TestSecret = "Bob's key"
            };
            settingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestSecret"] != "Bob's key"));
        }

        [TestCase("not base64 at all")]
        [TestCase("AQIDBA==")]
        [Platform("Win")]
        public void SecretThatCantBeDecryptedIsEmptyOnWindows(string stored)
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string> { ["TestSecret"] = stored });
            var settings = new TestSettings(settingsFile);
            Assert.That(settings.TestSecret, Is.Empty);
        }
    }
}
