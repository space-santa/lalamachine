using System;
using LibLala.FileExporter;
using NUnit.Framework;

namespace LibLala.Unittests.FileExporter
{
    public class MockExporterDirectory : IExporterDirectory
    {
        private bool _replyOfExists;
        private string _argOfCreateDirectory = "";

        public bool ReplyOfExists { get => _replyOfExists; set => _replyOfExists = value; }
        public string ArgOfCreateDirectory { get => _argOfCreateDirectory; set => _argOfCreateDirectory = value; }

        public bool Exists(string destination)
        {
            return ReplyOfExists;
        }

        public void CreateDirectory(string destination)
        {
            ArgOfCreateDirectory = destination;
        }

        public string GetDirectorySuffix()
        {
            return "-20180304_112233";
        }
    }
    public class DirectoryPreparerTest
    {
        [Test]
        public void PrepareDirectory_DoesNotExist_IsCreated()
        {
            var mock = new MockExporterDirectory
            {
                ReplyOfExists = false
            };
            var directroyPreparer = new DirectoryPreparer(mock);
            var destination = @"C:\the\path";
            directroyPreparer.PrepareDestination(destination);
            Assert.That(destination == mock.ArgOfCreateDirectory);
        }

        [Test]
        public void PrepareDirectory_DoesExist_IsCreated()
        {
            var mock = new MockExporterDirectory
            {
                ReplyOfExists = true
            };
            var directroyPreparer = new DirectoryPreparer(mock);
            var destination = @"C:\the\path";
            var expected = @"C:\the\path-20180304_112233";
            directroyPreparer.PrepareDestination(destination);
            Assert.That(expected == mock.ArgOfCreateDirectory);
        }

        [Test]
        public void ExporterDirectory_GetDirectorySuffix_IsProperlyFormattedCurrentDateTime()
        {
            // A clock of its own: read twice, the real one can be in the next second the second time.
            var exporterDirectory = new ExporterDirectory(new FixedTime(new DateTimeOffset(2018, 3, 4, 11, 22, 33, TimeSpan.Zero)));
            Assert.That(exporterDirectory.GetDirectorySuffix(), Is.EqualTo("-20180304_112233"));
        }

        private sealed class FixedTime(DateTimeOffset now) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => now;

            public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        }
    }
}
