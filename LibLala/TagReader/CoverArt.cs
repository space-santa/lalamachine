using System.Linq;

namespace LibLala.LibLalaTagReader
{
    // The cover picture in a track's tags.
    internal static class CoverArt
    {
        // The image as it's stored, like a JPEG. Null when the track has none.
        public static byte[]? Of(TagLib.Tag tag)
        {
            // An attachment that isn't a picture isn't one, like Mixed In Key's key and cue points or Serato's markers.
            var pictures = tag.Pictures.Where(picture => picture.Type != TagLib.PictureType.NotAPicture && picture.Data.Count > 0).ToList();
            // The front cover, not the publisher's logo next to it. Else what there is.
            var cover = pictures.Find(picture => picture.Type == TagLib.PictureType.FrontCover) ?? pictures.FirstOrDefault();
            return cover?.Data.Data;
        }
    }
}
