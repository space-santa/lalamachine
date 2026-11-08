using System;
using System.IO;
using System.Text;
using LibLala.LibLalaTagReader;
using NUnit.Framework;
using TestHelpers;

namespace LibLala.Unittests.Tagreader
{
    public class CoverArtTest
    {
        private static readonly byte[] Cover = [0xFF, 0xD8, 0xFF, 0xE0, 1];
        private static readonly byte[] Logo = [0xFF, 0xD8, 0xFF, 0xE0, 2];

        private string _path = "";

        [SetUp]
        public void SetUp()
        {
            _path = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}.mp3");
            TestMp3.Write(_path, "Title");
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_path);
        }

        [Test]
        public void Read_OfATrackWithoutPictures_IsNull()
        {
            Assert.That(CoverArt.Read(_path), Is.Null);
        }

        [Test]
        public void Read_GivesTheFrontCover_NotThePictureBeforeIt()
        {
            AddPictures(Picture(TagLib.PictureType.PublisherLogo, Logo), Picture(TagLib.PictureType.FrontCover, Cover));

            Assert.That(CoverArt.Read(_path), Is.EqualTo(Cover));
        }

        [Test]
        public void Read_OfATrackWithoutAFrontCover_GivesAnotherPicture()
        {
            AddPictures(Picture(TagLib.PictureType.Other, Cover));

            Assert.That(CoverArt.Read(_path), Is.EqualTo(Cover));
        }

        [Test]
        public void Read_DoesntGiveAnAttachmentThatIsntAPicture()
        {
            // Like the key that Mixed In Key writes.
            AddPictures(new TagLib.Picture(new TagLib.ByteVector(Encoding.ASCII.GetBytes("eyJrZXkiOiIyQSJ9")))
            {
                Type = TagLib.PictureType.NotAPicture,
                MimeType = "application/json",
                Description = "Key",
            });

            Assert.That(CoverArt.Read(_path), Is.Null);
        }

        [Test]
        public void Read_OfAFileThatIsntAudio_IsNull()
        {
            TestMp3.WriteBroken(_path);

            Assert.That(CoverArt.Read(_path), Is.Null);
        }

        [Test]
        public void Read_OfAFileThatIsntThere_IsNull()
        {
            File.Delete(_path);

            Assert.That(CoverArt.Read(_path), Is.Null);
        }

        private static TagLib.Picture Picture(TagLib.PictureType type, byte[] data) =>
            new(new TagLib.ByteVector(data)) { Type = type, MimeType = "image/jpeg" };

        private void AddPictures(params TagLib.Picture[] pictures)
        {
            using var file = TagLib.File.Create(_path);
            var id3 = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2, create: true);
            foreach (var picture in pictures)
            {
                id3.AddFrame(new TagLib.Id3v2.AttachmentFrame(picture));
            }
            file.Save();
        }
    }
}
