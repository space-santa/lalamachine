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
