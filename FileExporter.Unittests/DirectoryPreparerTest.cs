using System;
using Xunit;

namespace FileExporter.Unittest
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
        [Fact]
        public void PrepareDirectory_DoesNotExist_IsCreated()
        {
            var mock = new MockExporterDirectory();
            mock.ReplyOfExists = false;
            var directroyPreparer = new DirectoryPreparer(mock);
            string destination = @"C:\the\path";
            directroyPreparer.PrepareDestination(destination);
            Assert.Equal(destination, mock.ArgOfCreateDirectory);
        }

        [Fact]
        public void PrepareDirectory_DoesExist_IsCreated()
        {
            var mock = new MockExporterDirectory();
            mock.ReplyOfExists = true;
            var directroyPreparer = new DirectoryPreparer(mock);
            string destination = @"C:\the\path";
            string expected = @"C:\the\path-20180304_112233";
            directroyPreparer.PrepareDestination(destination);
            Assert.Equal(expected, mock.ArgOfCreateDirectory);
        }

        [Fact]
        public void ExporterDirectory_GetDirectorySuffix_IsProperlyFormattedCurrentDateTime()
        {
            var expected = DateTime.Now.ToString("-yyyyMMdd_HHmmss");
            var exporterDirectory = new ExporterDirectory();
            Assert.Equal(expected, exporterDirectory.GetDirectorySuffix());
        }
    }
}
