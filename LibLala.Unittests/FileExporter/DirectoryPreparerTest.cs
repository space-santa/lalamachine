using System;
using LibLala.FileExporter;
using NUnit.Framework;

namespace LibLala.Unittests.FileExporter
{
    public class MockExporterDirectory : IExporterDirectory
    {
        private bool replyOfExists = false;
        private string argOfCreateDirectory = "";

        public bool ReplyOfExists { get => replyOfExists; set => replyOfExists = value; }
        public string ArgOfCreateDirectory { get => argOfCreateDirectory; set => argOfCreateDirectory = value; }

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
            Assert.AreEqual(destination, mock.ArgOfCreateDirectory);
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
            Assert.AreEqual(expected, mock.ArgOfCreateDirectory);
        }

        [Test]
        public void ExporterDirectory_GetDirectorySuffix_IsProperlyFormattedCurrentDateTime()
        {
            var expected = DateTime.Now.ToString("-yyyyMMdd_HHmmss", LibLala.Constants.CULTURE);
            var exporterDirectory = new ExporterDirectory();
            Assert.AreEqual(expected, exporterDirectory.GetDirectorySuffix());
        }
    }
}
