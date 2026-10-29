using System.IO;

namespace TestHelpers
{
    internal static class TestMp3
    {
        /// <summary>
        /// Writes about three seconds of silence (MPEG-1 Layer III, 128 kbps, 44.1 kHz), tagged only when a title is given.
        /// </summary>
        public static void Write(string path, string? title = null, string? artist = null, string? album = null)
        {
            var frame = new byte[417];
            frame[0] = 0xFF;
            frame[1] = 0xFB;
            frame[2] = 0x90;
            frame[3] = 0x64;

            using (var stream = File.Create(path))
            {
                for (var i = 0; i < 120; i++)
                {
                    stream.Write(frame);
                }
            }

            if (title is null)
            {
                return;
            }

            using var file = TagLib.File.Create(path);
            file.Tag.Title = title;
            if (artist is { })
            {
                file.Tag.Performers = new[] { artist };
            }
            if (album is { })
            {
                file.Tag.Album = album;
            }
            file.Save();
        }

        /// <summary>
        /// Writes a file with an mp3 extension that isn't an mp3.
        /// </summary>
        public static void WriteBroken(string path)
        {
            File.WriteAllBytes(path, new byte[4096]);
        }
    }
}
