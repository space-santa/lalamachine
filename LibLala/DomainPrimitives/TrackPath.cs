using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LibLala.DomainPrimitives
{
    internal class TrackPath
    {
        private readonly FileInfo _fileInfo;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public TrackPath(string path)
        {
            _fileInfo = new FileInfo(path);
            if (!_fileInfo.Exists) { throw new ArgumentException(paramName: nameof(path), message: "File doesn't exist."); }

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
}
