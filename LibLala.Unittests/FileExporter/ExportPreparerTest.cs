using System;
using LibLala.FileExporter;
using NUnit.Framework;

namespace LibLala.Unittests.FileExporter
{
    public class ExportPreparerTest
    {
        private static string GetPath()
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

        [Test]
        public void GetNewFilename_9Tracks_NoLeadingZeroInFilename()
        {
            var result = ExportPreparer.GetNewFilename(3, 9, GetPath());
            var expected = "3-lala.mp3";
            Assert.That(expected == result);
        }

        [Test]
        public void GetNewFilename_12Tracks_LeadingZeroInFilename()
        {
            var result = ExportPreparer.GetNewFilename(3, 12, GetPath());
            var expected = "03-lala.mp3";
            Assert.That(expected == result);
        }

        [Test]
        public void GetNewFilename_120Tracks_LeadingZerosInFilename()
        {
            var result = ExportPreparer.GetNewFilename(3, 120, GetPath());
            var expected = "003-lala.mp3";
            Assert.That(expected == result);
        }
    }
}
