using System.Collections.Generic;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace Settings.Unittests
{
    internal class TestSettings : AppSettingsBase
    {
        public TestSettings(SettingsFile settingsFile) : base(settingsFile) { }

        public int TestInt { get => GetInt(); set => Set(value); }
        public bool TestBool { get => GetBool(); set => Set(value); }
        public double TestDouble { get => GetDouble(); set => Set(value); }
        public string TestString { get => GetString(); set => Set(value); }
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
    }
}
