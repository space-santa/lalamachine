using System;
using System.IO;

namespace LibLala.DomainPrimitives
{
    public interface ITrackPath
    {
        public string ToString();
        public string FullName { get; }
    }

    public class TrackPath : ITrackPath
    {
        private readonly FileInfo _fileInfo;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public TrackPath(string path)
        {
            _fileInfo = new FileInfo(path);
            if (!_fileInfo.Exists) { throw new ArgumentException(paramName: nameof(path), message: $"File {path} doesn't exist."); }

            var suffix = _fileInfo.Extension;
            var isMp3 = string.Equals(suffix, ".mp3", StringComparison.OrdinalIgnoreCase);
            var isM4a = string.Equals(suffix, ".m4a", StringComparison.OrdinalIgnoreCase);
            if (!(isMp3 || isM4a))
            {
                throw new ArgumentException(paramName: nameof(path), message: "File is neither mp3 nor m4a");
            }
        }

        public override string ToString() => _fileInfo.ToString();
        public string FullName => _fileInfo.FullName;
    }

    public class FakeTrackPath : ITrackPath
    {
        public FakeTrackPath(string path) { FullName = path; }
        public override string ToString() => FullName;
        public string FullName { get; }
    }
}
