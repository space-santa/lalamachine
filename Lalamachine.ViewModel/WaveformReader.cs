using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibLala.Audio;
using LibLala.Audio.Analysis;

namespace Lalamachine.ViewModel
{
    // Reads the waveforms of tracks in the background, each on a thread of its own. Keeps the last few: for a track that
    // plays again, and for the one mixed in, which is read before it takes over. Used from the UI thread only.
    public sealed class WaveformReader : IDisposable
    {
        private const int Kept = 4;

        private readonly Func<string, IAudioDecoder> _openDecoder;
        // The one asked for last at the end. A read still going on is stopped when it drops out.
        private readonly List<(string Path, Task<Waveform?> Waveform, CancellationTokenSource Reading)> _kept = [];

        public WaveformReader(Func<string, IAudioDecoder> openDecoder)
        {
            _openDecoder = openDecoder;
        }

        // Null when the track can't be read. Then it's read again the next time it's asked for, the drive may be back.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "Only the result of a read that's over, it doesn't block.")]
        public Task<Waveform?> Read(string path)
        {
            var index = _kept.FindIndex(kept => kept.Path == path);
            if (index >= 0)
            {
                var kept = _kept[index];
                _kept.RemoveAt(index);
                if (!kept.Waveform.IsCompletedSuccessfully || kept.Waveform.Result is not null)
                {
                    _kept.Add(kept);
                    return kept.Waveform;
                }
                kept.Reading.Dispose();
            }

            var reading = new CancellationTokenSource();
            // The token, not the source: it's disposed when the read drops out.
            var token = reading.Token;
            var waveform = new TaskCompletionSource<Waveform?>(TaskCreationOptions.RunContinuationsAsynchronously);
            new Thread(() => waveform.SetResult(ReadFile(path, token))) { IsBackground = true, Name = "Waveform" }.Start();
            _kept.Add((path, waveform.Task, reading));
            if (_kept.Count > Kept)
            {
                Stop(_kept[0].Reading);
                _kept.RemoveAt(0);
            }
            return waveform.Task;
        }

        public void Dispose()
        {
            foreach (var kept in _kept)
            {
                Stop(kept.Reading);
            }
            _kept.Clear();
        }

        private static void Stop(CancellationTokenSource reading)
        {
            reading.Cancel();
            reading.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An exception on a thread of its own would end the app. Without its waveform the track only shows a line.")]
        private Waveform? ReadFile(string path, CancellationToken token)
        {
            try
            {
                using var decoder = _openDecoder(path);
                return Waveform.Read(decoder, token);
            }
            catch (Exception)
            {
                // Stopped, a file that can't be read now, or a decoder's surprise.
                return null;
            }
        }
    }
}
