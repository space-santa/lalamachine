using System;
using System.IO;
using System.Linq;

namespace LibLala.LibLalaTagReader
{
    // The cover picture in a track's tags.
    public static class CoverArt
    {
        // The image as it's stored, like a JPEG. Null when the track has none or can't be read.
        public static byte[]? Read(string path)
        {
            try
            {
                using var file = TagLib.File.Create(path);
                // An attachment that isn't a picture isn't one, like Mixed In Key's key and cue points or Serato's markers.
                var pictures = file.Tag.Pictures.Where(picture => picture.Type != TagLib.PictureType.NotAPicture && picture.Data.Count > 0).ToList();
                // The front cover, not the publisher's logo next to it. Else what there is.
                var cover = pictures.Find(picture => picture.Type == TagLib.PictureType.FrontCover) ?? pictures.FirstOrDefault();
                return cover?.Data.Data;
            }
            catch (Exception e) when (e is TagLib.CorruptFileException or TagLib.UnsupportedFormatException or IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }
    }
}
