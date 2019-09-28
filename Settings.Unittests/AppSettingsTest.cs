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
            var SettingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(SettingsFile)
            {
                TestInt = 5
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestInt"] == "5"));
        }

        [Test]
        public void TestBoolShouldSave()
        {
            var SettingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(SettingsFile)
            {
                TestBool = true
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestBool"] == true.ToString(LibLala.Constants.CULTURE)));
        }

        [Test]
        public void TestDoubleShouldSave()
        {
            var SettingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(SettingsFile)
            {
                TestDouble = 5.5
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestDouble"] == "5.5"));
        }

        [Test]
        public void TestStringShouldSave()
        {
            var SettingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            var settings = new TestSettings(SettingsFile)
            {
                TestString = "Bob goes to town."
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestString"] == "Bob goes to town."));
        }
    }
}
