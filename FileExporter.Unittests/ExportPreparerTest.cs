using System;
using Xunit;

namespace FileExporter.Unittest
{
    public class ExportPreparerTest
    {
        private string GetPath()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                return @"C:\the\path\lala.mp3";
            }
            else
            {
                return @"/the/path/lala.mp3";
            }
        }

        [Fact]
        public void GetNewFilename_9Tracks_NoLeadingZeroInFilename()
        {
            string result = ExportPreparer.GetNewFilename(3, 9, GetPath());
            string expected = "3-lala.mp3";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetNewFilename_12Tracks_LeadingZeroInFilename()
        {
            string result = ExportPreparer.GetNewFilename(3, 12, GetPath());
            string expected = "03-lala.mp3";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetNewFilename_120Tracks_LeadingZerosInFilename()
        {
            string result = ExportPreparer.GetNewFilename(3, 120, GetPath());
            string expected = "003-lala.mp3";
            Assert.Equal(expected, result);
        }
    }
}
