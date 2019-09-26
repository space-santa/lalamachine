using System;
using LibLala.FileExporter;
using NUnit.Framework;

namespace LibLala.Unittests.FileExporter
{
    public class MockExporterDirectory : IExporterDirectory
    {
        public bool ReplyOfExists = false;
        public string ArgOfCreateDirectory = "";

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
            var expected = DateTime.Now.ToString("-yyyyMMdd_HHmmss");
            var exporterDirectory = new ExporterDirectory();
            Assert.AreEqual(expected, exporterDirectory.GetDirectorySuffix());
        }
    }
}
