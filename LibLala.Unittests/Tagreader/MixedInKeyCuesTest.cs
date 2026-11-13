using System;
using System.IO;
using System.Text;
using LibLala.LibLalaTagReader;
using NUnit.Framework;
using TestHelpers;

namespace LibLala.Unittests.Tagreader
{
    public class MixedInKeyCuesTest
    {
        private static string Base64(string json) => Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        [Test]
        public void Parse_GivesTheTimesInSeconds()
        {
            var cues = MixedInKeyCues.Parse(Base64("""{"cues":[{"time":49.0,"name":""},{"time":26299.0,"name":""}],"source":"mixedinkey","algorithm":14}"""));

            Assert.That(cues, Is.EqualTo(new[] { 0.049, 26.299 }).Within(1e-9));
        }

        [TestCase("not base64 at all!")]
        [TestCase("eyJrZXkiOiIyQSJ9")]
        public void Parse_OfSomethingElse_IsNull(string text)
        {
            Assert.That(MixedInKeyCues.Parse(text), Is.Null);
        }

        [Test]
        public void Read_OfATrackWithoutCues_IsEmpty()
        {
            var path = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}.mp3");
            try
            {
                TestMp3.Write(path, "Title");
                Assert.That(MixedInKeyCues.Read(path), Is.Empty);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Read_TakesTheCuesFromTheirObject_NotFromTheCover()
        {
            var path = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}.mp3");
            try
            {
                TestMp3.Write(path, "Title");
                using (var file = TagLib.File.Create(path))
                {
                    var id3 = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2, create: true);
                    id3.AddFrame(Attachment(TagLib.PictureType.FrontCover, """{"cues":[{"time":1000.0}]}"""));
                    id3.AddFrame(Attachment(TagLib.PictureType.NotAPicture, """{"cues":[{"time":49.0}]}"""));
                    file.Save();
                }

                Assert.That(MixedInKeyCues.Read(path), Is.EqualTo(new[] { 0.049 }).Within(1e-9));
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static TagLib.Id3v2.AttachmentFrame Attachment(TagLib.PictureType type, string json) =>
            new(new TagLib.Picture(new TagLib.ByteVector(Encoding.ASCII.GetBytes(Base64(json))))
            {
                Type = type,
                MimeType = "application/octet-stream",
                Description = "CuePoints",
            });

        [Test]
        public void Read_OfAFileThatIsntAudio_IsEmpty()
        {
            var path = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}.mp3");
            try
            {
                TestMp3.WriteBroken(path);
                Assert.That(MixedInKeyCues.Read(path), Is.Empty);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
