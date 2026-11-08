using System;
using System.IO;
using System.Runtime.Versioning;

namespace LibLala.Audio.Windows
{
    // Decodes with Windows's own codecs through Media Foundation, which read mp3 and m4a (AAC and ALAC), and converts to
    // AudioFormat. An mp3 goes frame by frame through Windows's MP3 decoder, for seeks that land where reading on would;
    // anything else through Media Foundation's source reader.
    [SupportedOSPlatform("windows")]
    public sealed class MediaFoundationDecoder : IAudioDecoder
    {
        private readonly IAudioDecoder _decoder;

        public MediaFoundationDecoder(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            MediaFoundation.Start(path);
            // An mp3 that isn't one, maybe MPEG Layer II, the source reader tries.
            var isMp3 = string.Equals(Path.GetExtension(path), ".mp3", StringComparison.OrdinalIgnoreCase);
            _decoder = (isMp3 ? Mp3Decoder.TryOpen(path) : null) ?? (IAudioDecoder)new SourceReaderDecoder(path);
        }

        public long Length => _decoder.Length;

        public int Read(Span<float> buffer) => _decoder.Read(buffer);

        public void Seek(long frame) => _decoder.Seek(frame);

        public void Dispose() => _decoder.Dispose();
    }
}
