using NUnit.Framework;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System.Collections.Generic;

namespace Settings.Unittests
{
    class TestSettings : AppSettingsBase
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
            SettingsFile SettingsFile = Substitute.For<SettingsFile>();
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            TestSettings settings = new TestSettings(SettingsFile)
            {
                TestInt = 5
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestInt"] == "5"));
        }

        [Test]
        public void TestBoolShouldSave()
        {
            SettingsFile SettingsFile = Substitute.For<SettingsFile>();
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            TestSettings settings = new TestSettings(SettingsFile)
            {
                TestBool = true
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestBool"] == true.ToString()));
        }

        [Test]
        public void TestDoubleShouldSave()
        {
            SettingsFile SettingsFile = Substitute.For<SettingsFile>();
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            TestSettings settings = new TestSettings(SettingsFile)
            {
                TestDouble = 5.5
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestDouble"] == "5.5"));
        }

        [Test]
        public void TestStringShouldSave()
        {
            SettingsFile SettingsFile = Substitute.For<SettingsFile>();
            SettingsFile.Load().Returns(new Dictionary<string, string>());
            TestSettings settings = new TestSettings(SettingsFile)
            {
                TestString = "Bob goes to town."
            };
            SettingsFile.Received().Save(Arg.Is<Dictionary<string, string>>(x => x["TestString"] == "Bob goes to town."));
        }
    }
}
