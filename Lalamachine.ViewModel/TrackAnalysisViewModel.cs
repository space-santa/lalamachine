using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LalaDb.Model;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    // What the analysis found in a track whose tags have no key, or no tempo: null for the one that the tags have, and for a
    // key when there is none to find, 0 for a tempo then.
    public sealed class TrackAnalyzedEventArgs : EventArgs
    {
        public TrackAnalyzedEventArgs(string path, string? initialKey, uint? beatsPerMinute)
        {
            Path = path;
            InitialKey = initialKey;
            BeatsPerMinute = beatsPerMinute;
        }

        public string Path { get; }
        public string? InitialKey { get; }
        public uint? BeatsPerMinute { get; }
    }

    // Finds the beats of the library's tracks that aren't analyzed yet, and the key of the ones whose tags have none, and
    // reads their waveforms, in the background on threads of low priority. Stops for a scan and starts again after it. Its
    // properties change and its event is raised on the thread that created it, i.e. the UI thread.
    public sealed class TrackAnalysisViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        private readonly TrackAnalysisStore _store;
        private readonly Func<string, IAudioDecoder> _openDecoder;
        private readonly int _threadCount;
        private readonly SynchronizationContext _uiContext;
        private CancellationTokenSource? _run;
        private int _toAnalyze;
        private int _analyzed;
        private bool _isAnalyzing;

        public TrackAnalysisViewModel(TrackAnalysisStore store, Func<string, IAudioDecoder> openDecoder, int threadCount)
        {
            _store = store;
            _openDecoder = openDecoder;
            _threadCount = Math.Max(1, threadCount);
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            private set
            {
                _isAnalyzing = value;
                NotifyPropertyChanged();
            }
        }

        public string Status => _toAnalyze > 0 ? $"Finding the beats, keys and waveforms of the tracks: {_analyzed} of {_toAnalyze}" : "Finding the beats, keys and waveforms of the tracks";

        // For the lists that show the track: its tags have no key or tempo, this is what there is to show instead.
        public event EventHandler<TrackAnalyzedEventArgs>? TrackAnalyzed;

        // Stops a run that is going on and starts over with what isn't analyzed now.
        public void Start()
        {
            Stop();
            var run = new CancellationTokenSource();
            _run = run;
            _toAnalyze = 0;
            _analyzed = 0;
            IsAnalyzing = true;
            NotifyPropertyChanged(nameof(Status));
            // The token, not the source: Stop disposes that.
            var token = run.Token;
            new Thread(() => Run(run, token)) { IsBackground = true, Priority = ThreadPriority.BelowNormal, Name = "Beat analysis" }.Start();
        }

        // The tracks being analyzed are finished first, in the background.
        public void Stop()
        {
            if (_run is null)
            {
                return;
            }
            _run.Cancel();
            _run.Dispose();
            _run = null;
            IsAnalyzing = false;
        }

        public void ScanningChangedHandler(object? sender, ScanningChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.Scanning)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }

        public void Dispose() => Stop();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An exception on a thread of its own would end the app. Without the analysis tracks only play without transitions.")]
        private void Run(CancellationTokenSource run, CancellationToken token)
        {
            IReadOnlyList<TrackToAnalyze> tracks;
            try
            {
                tracks = _store.TracksToAnalyze();
            }
            catch (Exception)
            {
                tracks = [];
            }

            PostForRun(run, () =>
            {
                _toAnalyze = tracks.Count;
                NotifyPropertyChanged(nameof(Status));
            });

            var queue = new ConcurrentQueue<TrackToAnalyze>(tracks);
            var workers = Enumerable.Range(0, _threadCount)
                .Select(i => new Thread(() => Work(queue, run, token)) { IsBackground = true, Priority = ThreadPriority.BelowNormal, Name = $"Beat analysis {i}" })
                .ToList();
            workers.ForEach(worker => worker.Start());
            workers.ForEach(worker => worker.Join());

            PostForRun(run, () =>
            {
                _run = null;
                run.Dispose();
                IsAnalyzing = false;
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An exception on a thread of its own would end the app. A track that fails is tried again next time.")]
        private void Work(ConcurrentQueue<TrackToAnalyze> queue, CancellationTokenSource run, CancellationToken token)
        {
            while (!token.IsCancellationRequested && queue.TryDequeue(out var track))
            {
                TrackAnalyzedEventArgs? found = null;
                try
                {
                    var (grid, key, waveform) = Analyze(track);
                    _store.Save(track, grid, key, waveform);
                    found = Found(track, grid, key);
                }
                catch (IOException)
                {
                    // Not saved, it is tried again next time. The drive may be back then.
                }
                catch (Exception)
                {
                    // Not saved either, a database that is busy for too long or a decoder's surprise.
                }

                PostForRun(run, () =>
                {
                    _analyzed++;
                    NotifyPropertyChanged(nameof(Status));
                    if (found is not null)
                    {
                        TrackAnalyzed?.Invoke(this, found);
                    }
                });
            }
        }

        private (BeatGrid? Grid, MusicalKey? Key, Waveform? Waveform) Analyze(TrackToAnalyze track)
        {
            var waveform = track.NeedsWaveform ? new WaveformBuilder() : null;
            // Opened first: a file that can't be read now isn't saved, and is tried again next time. The waveform is built from
            // what the other analyses read, so that the track is decoded once.
            using var decoder = WithWaveform(_openDecoder(track.Path), waveform);
            var keyAnalyzer = track.NeedsKey ? new KeyAnalyzer() : null;
            IReadOnlyList<double> cues = track.NeedsGrid ? MixedInKeyCues.Read(track.Path) : [];
            // What an earlier version found, when it can't have changed, is what this one finds: the track isn't decoded for
            // it.
            var isEarlierCurrent = track.Earlier is not null && !BeatGridAnalyzer.MayChange(track.Earlier, track.EarlierVersion, cues);
            var grid = isEarlierCurrent ? track.Earlier : null;
            if (track.NeedsGrid && !isEarlierCurrent)
            {
                // The key analysis hears what the beat analysis reads, so that the track is decoded once.
                grid = BeatGridAnalyzer.Analyze(decoder, track.TaggedBeatsPerMinute > 0 ? track.TaggedBeatsPerMinute : null, cues, keyAnalyzer);
            }
            else if (keyAnalyzer is not null)
            {
                keyAnalyzer.Read(decoder);
            }
            else if (waveform is not null)
            {
                ReadToEnd(decoder);
            }
            return (grid, keyAnalyzer?.Key(), waveform?.Build());
        }

        private static IAudioDecoder WithWaveform(IAudioDecoder decoder, WaveformBuilder? waveform) => waveform is null ? decoder : new WaveformDecoder(decoder, waveform);

        private static void ReadToEnd(IAudioDecoder decoder)
        {
            var block = new float[8192 * AudioFormat.Channels];
            int read;
            do
            {
                read = decoder.Read(block);
            }
            while (read > 0);
        }

        // Null when the tags have both, or what they don't have wasn't looked for this time. Without a key that wasn't found:
        // a track that has none shows none anyway, and one whose tags have something else than a key keeps showing that.
        private static TrackAnalyzedEventArgs? Found(TrackToAnalyze track, BeatGrid? grid, MusicalKey? key)
        {
            var initialKey = track.NeedsKey ? key?.Camelot : null;
            uint? beatsPerMinute = track.NeedsGrid && track.TaggedBeatsPerMinute <= 0 ? grid?.ShownBeatsPerMinute ?? 0 : null;
            return initialKey is null && beatsPerMinute is null ? null : new TrackAnalyzedEventArgs(track.Path, initialKey, beatsPerMinute);
        }

        // What comes from a run that was stopped since is dropped. Its source may be disposed, it is only compared.
        private void PostForRun(CancellationTokenSource run, Action action)
        {
            _uiContext.Post(_ =>
            {
                if (ReferenceEquals(run, _run))
                {
                    action();
                }
            }, null);
        }

        // Builds the waveform from what's read from the decoder, which it disposes of.
        private sealed class WaveformDecoder : IAudioDecoder
        {
            private readonly IAudioDecoder _decoder;
            private readonly WaveformBuilder _waveform;

            public WaveformDecoder(IAudioDecoder decoder, WaveformBuilder waveform)
            {
                _decoder = decoder;
                _waveform = waveform;
            }

            public long Length => _decoder.Length;

            public int Read(Span<float> buffer)
            {
                var read = _decoder.Read(buffer);
                _waveform.Add(buffer[..(read * AudioFormat.Channels)]);
                return read;
            }

            // Not done by the analyses: the waveform would have a part twice, or not at all.
            public void Seek(long frame) => throw new NotSupportedException();

            public void Dispose() => _decoder.Dispose();
        }
    }
}
