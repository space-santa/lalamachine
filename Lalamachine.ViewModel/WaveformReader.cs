using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LalaDb.Model;
using LibLala.Audio;
using LibLala.Audio.Analysis;

namespace Lalamachine.ViewModel
{
    // Reads the waveforms of the track that plays and the one mixed in, each on a thread of its own: the one the analysis
    // stored, or else from the file, and then it's stored. Used from the UI thread only.
    public sealed class WaveformReader : IDisposable
    {
        // The track that plays and the one mixed in, which is read before it takes over. A read still going on is stopped
        // when it drops out: the user went on to another track.
        private const int Kept = 2;

        private readonly Func<string, IAudioDecoder> _openDecoder;
        private readonly Func<string, Waveform?> _findStored;
        private readonly Action<string, FileStamp, Waveform> _store;
        // The one asked for last at the end.
        private readonly List<(string Path, Task<Waveform?> Waveform, CancellationTokenSource Reading)> _kept = [];

        public WaveformReader(Func<string, IAudioDecoder> openDecoder, Func<string, Waveform?> findStored, Action<string, FileStamp, Waveform> store)
        {
            _openDecoder = openDecoder;
            _findStored = findStored;
            _store = store;
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
            // Low, like the analysis: the player's threads come first.
            new Thread(() => waveform.SetResult(ReadFile(path, token))) { IsBackground = true, Priority = ThreadPriority.BelowNormal, Name = "Waveform" }.Start();
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
                if (_findStored(path) is { } stored)
                {
                    return stored;
                }
                // Of the file as it was before it was read, like the analysis's: one that changes meanwhile is read again.
                var stamp = FileStamp.Of(path);
                using var decoder = _openDecoder(path);
                var waveform = Waveform.Read(decoder, token);
                if (stamp is { } before)
                {
                    Store(path, before, waveform);
                }
                return waveform;
            }
            catch (Exception)
            {
                // Stopped, a file that can't be read now, or a decoder's surprise.
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The waveform is shown all the same.")]
        private void Store(string path, FileStamp stamp, Waveform waveform)
        {
            try
            {
                _store(path, stamp, waveform);
            }
            catch (Exception)
            {
                // A database that is busy for too long: read again the next time it plays, or by the analysis.
            }
        }
    }
}
