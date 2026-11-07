using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LalaDb.Model;
using LibLala.Audio;
using LibLala.Audio.Analysis;

namespace Lalamachine.ViewModel
{
    // Finds the beats of the library's tracks that aren't analyzed yet, in the background on threads of low priority. Stops
    // for a scan and starts again after it. Its properties change on the thread that created it, i.e. the UI thread.
    public sealed class BeatAnalysisViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        private readonly TrackAnalysisStore _store;
        private readonly Func<string, IAudioDecoder> _openDecoder;
        private readonly int _threadCount;
        private readonly SynchronizationContext _uiContext;
        // One SQLite connection writes at a time anyway, and this way two never wait for each other.
        private readonly Lock _saveGate = new();
        private CancellationTokenSource? _run;
        private int _toAnalyze;
        private int _analyzed;
        private bool _isAnalyzing;

        public BeatAnalysisViewModel(TrackAnalysisStore store, Func<string, IAudioDecoder> openDecoder, int threadCount)
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

        public string Status => _toAnalyze > 0 ? $"Finding the beats of the tracks: {_analyzed} of {_toAnalyze}" : "Finding the beats of the tracks";

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
                try
                {
                    BeatGrid? grid;
                    using (var decoder = _openDecoder(track.Path))
                    {
                        grid = BeatGridAnalyzer.Analyze(decoder, track.TaggedBeatsPerMinute > 0 ? track.TaggedBeatsPerMinute : null);
                    }
                    lock (_saveGate)
                    {
                        _store.Save(track.Path, track.Stamp, grid);
                    }
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
                });
            }
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
    }
}
