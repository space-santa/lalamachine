using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LibLala.Audio.Mixing;

namespace LibLala.Audio
{
    public sealed class TrackEventArgs : EventArgs
    {
        public TrackEventArgs(int track)
        {
            Track = track;
        }

        // The number Open or MixTo returned for the track.
        public int Track { get; }
    }

    // Plays a track through an output, which asks it for audio on its own thread, and mixes the next one into it like a DJ:
    // at the same tempo with the beats on top of each other, through a crossover that moves up from the bass, the one going
    // out above it and the one coming in below it.
    // The track going out is the current one until the transition is over: Position, Length and Seek are of it. A seek
    // into the transition is a seek in the mix, the incoming track moves along. The rest is called from one thread. The
    // events are raised on either, without holding a lock of the engine.
    public sealed class AudioEngine : IAudioRenderer, IDisposable
    {
        // The filters and gains change in steps this long, 5.8 ms.
        private const int StepFrames = 256;
        // TransitionDue comes this long before the earliest transition, for the next track to be opened in time.
        private const double DueLeadSeconds = 2;
        // After a transition the track comes back to its own tempo over this many bars.
        private const int TempoRampBars = 8;
        // While two tracks play, peaks above this are rounded off instead of clipping.
        private const float SoftClipFrom = 0.9f;
        // A seek goes no closer to the end of a transition, so that dragging the position slider can't finish it: the next
        // track would take over and the drag would go on in that one.
        private const double SeekMarginSeconds = 0.5;
        // When A is already past where the transition would be over, a short crossfade, but not shorter than this.
        private const double MinTransitionSeconds = 1;

        private readonly Lock _gate = new();
        private readonly IAudioOutput _output;
        // What Render made and the output didn't play yet, oldest first.
        private readonly Queue<Chunk> _rendered = new();
        private readonly float[] _deckBuffer = new float[StepFrames * AudioFormat.Channels];
        // What Render raises once it let go of the lock. Only used by Render, under the lock.
        private readonly List<(EventHandler<TrackEventArgs>? Handler, int Track)> _events = [];
        // The track that plays, and goes out in a transition.
        private Deck? _current;
        // During a transition, the track that comes in. It takes over when the transition is over.
        private Deck? _incoming;
        private Transition? _transition;
        private Ramp? _ramp;
        private volatile bool _clicks;
        private volatile bool _transitions;
        private int _track;
        // Frames rendered since the current track opened or moved. The transitions go by it.
        private long _frame;
        // Where the current track starts, until the output played some of it.
        private double _start;
        // Of what the output played: the track, where it is, and how long it is.
        private int _playedTrack;
        private long _playedPosition;
        private long _playedLength;
        private bool _hasPlayed;
        // Of the incoming track as played, while the transition runs.
        private double? _playedIncomingPosition;
        private long _playedIncomingLength;
        // Seconds of the current track at which TransitionDue is raised, NaN when it was.
        private double? _dueAt;
        private bool _isEndPlayed;
        private bool _isFailed;
        // Only used on the calling thread.
        private bool _isPlaying;

        public AudioEngine(Func<IAudioRenderer, IAudioOutput> createOutput)
        {
            ArgumentNullException.ThrowIfNull(createOutput);
            _output = createOutput(this);
        }

        // The output played the last frame of the current track. Raised once, until Seek or Play starts it again. Not for a
        // track that a transition took over from.
        public event EventHandler<TrackEventArgs>? Ended;
        // The track can't be read any more, or the output can't play. Raised once per track.
        public event EventHandler<TrackEventArgs>? Failed;
        // With Transitions on, the track gets close to where the next one would come in: time for MixTo. Raised once per
        // track, again after a seek back to before it.
        public event EventHandler<TrackEventArgs>? TransitionDue;
        // The transition to the track that MixTo returned the number of is over, as played: it's the current track now.
        public event EventHandler<TrackEventArgs>? TookOver;

        public float Volume
        {
            get => _output.Volume;
            set => _output.Volume = value;
        }

        // A click on every beat of the tracks' grids, to hear whether they're on the beat.
        public bool Clicks
        {
            get => _clicks;
            set => _clicks = value;
        }

        // Raises TransitionDue. Turned off, a transition that didn't start yet is called off, one that did goes on. Turned on
        // again, the next track can be due again.
        public bool Transitions
        {
            get => _transitions;
            set
            {
                Deck? calledOff = null;
                lock (_gate)
                {
                    _transitions = value;
                    if (!value && _transition is { IsStarted: false })
                    {
                        calledOff = _incoming;
                        _incoming = null;
                        _transition = null;
                    }
                    else if (value && _transition is null)
                    {
                        _dueAt = null;
                    }
                }
                calledOff?.Dispose();
            }
        }

        // Of the current track as played, in frames, 0 while none is open.
        public long Length
        {
            get
            {
                lock (_gate)
                {
                    return _hasPlayed ? _playedLength : _current?.Decoder.Length ?? 0;
                }
            }
        }

        // Of the current track, in frames. Of what was played, the output can have more waiting to be played.
        public long Position
        {
            get
            {
                lock (_gate)
                {
                    return _current is null ? 0 : _hasPlayed ? _playedPosition : (long)Math.Round(_start);
                }
            }
        }

        // Of the track that comes in, in frames, from when it's heard until it takes over. Null otherwise.
        public long? IncomingPosition
        {
            get
            {
                lock (_gate)
                {
                    return _playedIncomingPosition is { } position ? (long)Math.Round(position) : null;
                }
            }
        }

        public long? IncomingLength
        {
            get
            {
                lock (_gate)
                {
                    return _playedIncomingPosition is null ? null : _playedIncomingLength;
                }
            }
        }

        // Stops what plays and takes over the decoder. Returns the number of the track, the events carry it.
        public int Open(IAudioDecoder decoder) => Open(decoder, null);

        // With the track's beat grid, null when it has none or wasn't analyzed.
        public int Open(IAudioDecoder decoder, BeatGrid? grid)
        {
            ArgumentNullException.ThrowIfNull(decoder);
            return Replace(decoder, grid);
        }

        // Stops what plays and closes the track. Returns a new number, which no event carries.
        public int Close() => Replace(null, null);

        // Takes over the decoder and mixes its track into the current one, where the planner says, or where the transition
        // is when the current track is in it already. Past it, a short one. What came in before is dropped. Returns the
        // number of the track, TookOver carries it, or 0 without taking over the decoder when there's no track to mix into.
        // Throws an IOException when the decoder can't seek to where the track comes in, then nothing changed. A tempo that
        // is still coming back from the transition before stays where it is now, the plan goes by it.
        public int MixTo(IAudioDecoder decoder, BeatGrid? grid)
        {
            ArgumentNullException.ThrowIfNull(decoder);
            long frame;
            lock (_gate)
            {
                if (TrackToMixInto() is not { } outgoing)
                {
                    return 0;
                }
                frame = PlanMix(outgoing, grid).IncomingFrame;
            }

            // Not under the lock: a seek can read the file for longer than the output has audio queued. The deck isn't in
            // the mix yet, nothing else uses it.
            var incoming = new Deck(decoder, grid, _track + 1);
            incoming.Seek(frame);

            Deck? dropped;
            int track;
            lock (_gate)
            {
                if (TrackToMixInto() is not { } outgoing)
                {
                    return 0;
                }
                var (plan, progress, at) = PlanMix(outgoing, grid);
                if (at != frame)
                {
                    // In the transition already, the current track moved on meanwhile. Close to the seek before, quick.
                    incoming.Seek(at);
                }
                incoming.Tempo = plan.TempoB;
                track = ++_track;
                dropped = _incoming;
                _incoming = incoming;
                _transition = new Transition(plan, (long)Math.Round(plan.Seconds * AudioFormat.SampleRate));
                _ramp = null;
                if (progress is { } p)
                {
                    _transition.StartAt(_frame - (long)Math.Round(p * _transition.LengthFrames));
                }
            }
            dropped?.Dispose();
            return track;
        }

        public void Play()
        {
            bool isAtEnd;
            lock (_gate)
            {
                if (_current is null || _isFailed)
                {
                    return;
                }
                isAtEnd = _isEndPlayed;
            }

            // Played to the end: from the start again.
            if (isAtEnd)
            {
                _output.Reset();
                if (!MoveTo(0))
                {
                    return;
                }
            }
            _isPlaying = true;
            StartOutput();
        }

        public void Pause()
        {
            _isPlaying = false;
            _output.Pause();
        }

        // Pauses and gives up what was rendered but not played yet, the output lets the device go. Play continues where the
        // output stopped, or from the start after the end.
        public void Stop()
        {
            _isPlaying = false;
            _output.Pause();
            _output.Reset();
            bool isAtEnd;
            long position;
            lock (_gate)
            {
                isAtEnd = _current is null || _isFailed || _isEndPlayed;
                position = _hasPlayed ? _playedPosition : (long)Math.Round(_start);
            }
            if (!isAtEnd)
            {
                MoveTo(position);
            }
        }

        // Of the current track. Without a transition, it comes back to its own tempo at once. With one, it is a seek in the
        // mix: before it's due the transition is off, until it starts the incoming track waits, in it the incoming track is
        // where it would be, and it doesn't go so close to the end that the transition would be over.
        public void Seek(long frame)
        {
            _output.Reset();
            long length;
            lock (_gate)
            {
                if (_current is null || _isFailed)
                {
                    return;
                }
                length = _current.Decoder.Length;
            }

            // Only reset, the output keeps running and continues at once. Dragging the position slider seeks many times.
            if (!MoveTo(Math.Clamp(frame, 0, length)))
            {
                _isPlaying = false;
                _output.Pause();
            }
            else if (_isPlaying)
            {
                StartOutput();
            }
        }

        public void Render(Span<float> buffer)
        {
            // Only when there are any, this is the output's real-time thread.
            (EventHandler<TrackEventArgs>? Handler, int Track)[]? events = null;
            lock (_gate)
            {
                buffer.Clear();
                var frames = buffer.Length / AudioFormat.Channels;
                var done = 0;
                while (done < frames)
                {
                    var step = Math.Min(StepFrames, frames - done);
                    if (_transition is { IsStarted: false } waiting)
                    {
                        var until = FramesUntilStart(waiting);
                        if (until <= 0.5)
                        {
                            waiting.StartAt(_frame);
                        }
                        else if (until < step)
                        {
                            step = (int)Math.Round(until);
                        }
                    }

                    MixStep(buffer.Slice(done * AudioFormat.Channels, step * AudioFormat.Channels));
                    done += step;
                    _frame += step;
                    FinishTransition();
                    CheckDue();
                }

                var isEnd = _current is { IsEnded: true } && _transition is null;
                var incoming = _transition is { IsStarted: true } ? _incoming : null;
                _rendered.Enqueue(new Chunk(_current?.Track ?? 0, _current?.Position ?? 0, _current?.Decoder.Length ?? 0, isEnd, incoming?.Position, incoming?.Decoder.Length ?? 0));
                if (_events.Count > 0)
                {
                    events = [.. _events];
                    _events.Clear();
                }
            }

            foreach (var (handler, track) in events ?? [])
            {
                handler?.Invoke(this, new TrackEventArgs(track));
            }
        }

        public void Played()
        {
            int? endedTrack = null;
            int? tookOver = null;
            lock (_gate)
            {
                if (!_rendered.TryDequeue(out var chunk))
                {
                    return;
                }
                if (chunk.Track != _playedTrack)
                {
                    _playedTrack = chunk.Track;
                    tookOver = chunk.Track;
                }
                _playedPosition = (long)Math.Round(chunk.Position);
                _playedLength = chunk.Length;
                _hasPlayed = true;
                _playedIncomingPosition = chunk.IncomingPosition;
                _playedIncomingLength = chunk.IncomingLength;
                if (chunk.IsEnd && !_isEndPlayed)
                {
                    _isEndPlayed = true;
                    endedTrack = chunk.Track;
                }
            }

            if (tookOver is { } track)
            {
                TookOver?.Invoke(this, new TrackEventArgs(track));
            }
            if (endedTrack is { } ended)
            {
                Ended?.Invoke(this, new TrackEventArgs(ended));
            }
        }

        public void Dispose()
        {
            _output.Dispose();
            lock (_gate)
            {
                _incoming?.Dispose();
                _current?.Dispose();
                _incoming = null;
                _current = null;
            }
        }

        // The second of A that a seek goes to at most while the transition is planned.
        private static double SeekEnd(TransitionPlan plan, double tempoA, double lengthA) =>
            Math.Min(plan.StartA + (plan.Seconds * tempoA), lengthA) - SeekMarginSeconds;

        // How far into the transition A is at the second, from where it starts.
        private static double Progress(TransitionPlan plan, double tempoA, double seconds) =>
            (seconds - plan.StartA) / (plan.Seconds * tempoA);

        // The frame of B that plays that far into the transition: B moves on at its own tempo times the one it plays at.
        private static long IncomingFrame(TransitionPlan plan, double progress) =>
            (long)Math.Round((plan.StartB + (progress * plan.Seconds * plan.TempoB)) * AudioFormat.SampleRate);

        // The current track, when it plays on for something to be mixed into it.
        private Deck? TrackToMixInto() => _isFailed || _isEndPlayed ? null : _current;

        // How the track comes into the current one from where that is now, how far into the transition that is already,
        // null before it, and the frame the track comes in with.
        private static (TransitionPlan Plan, double? Progress, long IncomingFrame) PlanMix(Deck outgoing, BeatGrid? grid)
        {
            var length = outgoing.Decoder.Length / (double)AudioFormat.SampleRate;
            var now = outgoing.Position / AudioFormat.SampleRate;
            var plan = TransitionPlanner.Plan(nowA: 0, length, outgoing.Grid, outgoing.Tempo, grid);
            double? progress = null;
            if (now >= SeekEnd(plan, outgoing.Tempo, length))
            {
                // Past it already. A crossfade close to the end of the file would be too short, a cut is meant to be.
                plan = TransitionPlanner.Plan(now, length, outgoing.Grid, outgoing.Tempo, grid);
                if (plan.Kind == TransitionKind.Crossfade)
                {
                    plan = plan with { Seconds = Math.Max(plan.Seconds, MinTransitionSeconds) };
                }
            }
            else if (now >= plan.StartA)
            {
                progress = Progress(plan, outgoing.Tempo, now);
            }
            return (plan, progress, IncomingFrame(plan, progress ?? 0));
        }

        // One step: the incoming track and the current one, filtered and faded by how far the transition is.
        private void MixStep(Span<float> buffer)
        {
            var frames = buffer.Length / AudioFormat.Channels;
            var transition = _transition;
            var isRunning = transition is { IsStarted: true };
            var from = isRunning ? transition!.Progress(_frame) : 0;
            var to = isRunning ? transition!.Progress(_frame + frames) : 0;
            var kind = transition?.Plan.Kind ?? TransitionKind.Crossfade;
            var isFiltered = kind != TransitionKind.Cut;

            // First, so that it reads from where the step starts in the current track.
            if (_incoming is { } incoming && isRunning)
            {
                incoming.Tempo = transition!.Plan.TempoB;
                incoming.LowPass.Cutoff = isFiltered ? TransitionShape.Crossover((from + to) / 2) : incoming.LowPass.Off;
                var gain = (TransitionShape.IncomingGain(from, kind), TransitionShape.IncomingGain(to, kind));
                if (!RenderDeck(incoming, buffer, gain))
                {
                    // It can't be read any more, the current track plays on as if it had never come.
                    incoming.Dispose();
                    _incoming = null;
                    _transition = null;
                    transition = null;
                    isRunning = false;
                }
            }

            if (_current is { } current)
            {
                current.HighPass.Cutoff = isRunning && isFiltered ? TransitionShape.Crossover((from + to) / 2) : current.HighPass.Off;
                if (!isRunning && _ramp is { } ramp)
                {
                    current.Tempo = ramp.Tempo(_frame);
                    if (_frame >= ramp.EndFrame)
                    {
                        _ramp = null;
                    }
                }
                var gain = isRunning ? (TransitionShape.OutgoingGain(from, kind), TransitionShape.OutgoingGain(to, kind)) : (1.0, 1.0);
                if (!RenderDeck(current, buffer, gain))
                {
                    if (_incoming is not null && transition is not null)
                    {
                        // The incoming track takes over now.
                        transition.StartAt(_frame - transition.LengthFrames);
                    }
                    else if (!_isFailed)
                    {
                        _isFailed = true;
                        _events.Add((Failed, current.Track));
                    }
                }
                else if (transition is { IsStarted: false } && current.IsEnded)
                {
                    // It ended before it was planned to, maybe it's shorter than it said.
                    transition.StartAt(_frame + frames);
                }
            }

            if (isRunning && _incoming is not null)
            {
                SoftClip(buffer);
            }
        }

        // Adds the deck to the buffer, filtered, with the gain going from the first to the second. False when it can't be read.
        private bool RenderDeck(Deck deck, Span<float> buffer, (double From, double To) gain)
        {
            var samples = _deckBuffer.AsSpan(0, buffer.Length);
            var start = deck.Position;
            try
            {
                deck.Render(samples);
            }
            catch (IOException)
            {
                return false;
            }
            if (_clicks && deck.Grid is { } grid)
            {
                Metronome.Add(samples, start, deck.Tempo, grid);
            }
            deck.LowPass.Process(samples);
            deck.HighPass.Process(samples);

            var frames = buffer.Length / AudioFormat.Channels;
            for (var i = 0; i < frames; i++)
            {
                var g = (float)(gain.From + ((gain.To - gain.From) * i / frames));
                for (var c = 0; c < AudioFormat.Channels; c++)
                {
                    buffer[(i * AudioFormat.Channels) + c] += g * samples[(i * AudioFormat.Channels) + c];
                }
            }
            return true;
        }

        private double FramesUntilStart(Transition transition)
        {
            if (_current is not { } current)
            {
                return 0;
            }
            return ((transition.Plan.StartA * AudioFormat.SampleRate) - current.Position) / current.Tempo;
        }

        // At the end the current track is gone and the incoming one takes over, coming back to its own tempo.
        private void FinishTransition()
        {
            if (_transition is not { IsStarted: true } transition || _frame < transition.StartFrame + transition.LengthFrames)
            {
                return;
            }
            _current?.Dispose();
            _current = _incoming;
            _incoming = null;
            _transition = null;
            _dueAt = null;
            if (_current is { } current)
            {
                current.LowPass.Cutoff = current.LowPass.Off;
                if (current.Tempo != 1)
                {
                    var barFrames = 4 * 60.0 / ((current.Grid?.BeatsPerMinute ?? 120) * current.Tempo) * AudioFormat.SampleRate;
                    _ramp = new Ramp(_frame, _frame + (long)(TempoRampBars * barFrames), current.Tempo);
                }
            }
        }

        private void CheckDue()
        {
            if (!_transitions || _transition is not null || _current is not { IsEnded: false } current)
            {
                return;
            }
            _dueAt ??= DueAt(current);
            if (_dueAt is { } dueAt && !double.IsNaN(dueAt) && current.Position / AudioFormat.SampleRate >= dueAt)
            {
                _dueAt = double.NaN;
                _events.Add((TransitionDue, current.Track));
            }
        }

        private static double DueAt(Deck deck) => TransitionPlanner.EarliestStart(deck.Decoder.Length / (double)AudioFormat.SampleRate, deck.Grid) - DueLeadSeconds;

        private static void SoftClip(Span<float> buffer)
        {
            const float room = 1 - SoftClipFrom;
            for (var i = 0; i < buffer.Length; i++)
            {
                var x = buffer[i];
                var size = Math.Abs(x);
                if (size > SoftClipFrom)
                {
                    buffer[i] = MathF.CopySign(SoftClipFrom + (room * MathF.Tanh((size - SoftClipFrom) / room)), x);
                }
            }
        }

        private int Replace(IAudioDecoder? decoder, BeatGrid? grid)
        {
            _isPlaying = false;
            _output.Pause();
            _output.Reset();
            Deck? oldCurrent;
            Deck? oldIncoming;
            int track;
            lock (_gate)
            {
                oldCurrent = _current;
                oldIncoming = _incoming;
                track = ++_track;
                _current = decoder is null ? null : new Deck(decoder, grid, track);
                _incoming = null;
                _transition = null;
                _ramp = null;
                _isFailed = false;
                _isEndPlayed = false;
                _rendered.Clear();
                _frame = 0;
                _start = 0;
                _playedTrack = track;
                _hasPlayed = false;
                _playedIncomingPosition = null;
                _dueAt = null;
            }
            oldCurrent?.Dispose();
            oldIncoming?.Dispose();
            return track;
        }

        // Of the current track, only while the output is stopped. False when it failed.
        private bool MoveTo(long frame)
        {
            int track;
            int? tookOver = null;
            lock (_gate)
            {
                _rendered.Clear();
                _frame = 0;
                _isEndPlayed = false;
                _ramp = null;
                track = _current?.Track ?? 0;
                if (_current is not { } current)
                {
                    return true;
                }
                // It took over while the output had the end of the transition still to play. The frame is of the track
                // before, as it was played, e.g. from its position slider: this one goes on from where it was heard.
                if (_playedTrack != current.Track)
                {
                    _playedTrack = current.Track;
                    tookOver = current.Track;
                    frame = (long)Math.Round(_playedIncomingPosition ?? current.Position);
                }

                var seconds = frame / (double)AudioFormat.SampleRate;
                if (_transition is { } transition && _incoming is { } incoming)
                {
                    var plan = transition.Plan;
                    var length = current.Decoder.Length / (double)AudioFormat.SampleRate;
                    if (seconds < plan.StartA - DueLeadSeconds)
                    {
                        // Back to before it's due: no transition, until it's due again.
                        incoming.Dispose();
                        _incoming = null;
                        _transition = null;
                        _dueAt = null;
                        current.HighPass.Cutoff = current.HighPass.Off;
                    }
                    else
                    {
                        seconds = Math.Max(0, Math.Min(seconds, SeekEnd(plan, current.Tempo, length)));
                        frame = (long)Math.Round(seconds * AudioFormat.SampleRate);
                        double? progress = seconds >= plan.StartA ? Progress(plan, current.Tempo, seconds) : null;
                        try
                        {
                            incoming.Seek(IncomingFrame(plan, progress ?? 0));
                            if (progress is { } p)
                            {
                                transition.StartAt(-(long)Math.Round(p * transition.LengthFrames));
                            }
                            else
                            {
                                transition.Wait();
                                current.HighPass.Cutoff = current.HighPass.Off;
                            }
                        }
                        catch (IOException)
                        {
                            incoming.Dispose();
                            _incoming = null;
                            _transition = null;
                            current.HighPass.Cutoff = current.HighPass.Off;
                        }
                    }
                }
                else
                {
                    current.Tempo = 1;
                    current.HighPass.Cutoff = current.HighPass.Off;
                    current.LowPass.Cutoff = current.LowPass.Off;
                    _dueAt = null;
                }

                _start = frame;
                _hasPlayed = false;
                _playedIncomingPosition = null;
                try
                {
                    current.Seek(frame);
                }
                catch (IOException)
                {
                    _isFailed = true;
                }
            }

            if (tookOver is { } took)
            {
                TookOver?.Invoke(this, new TrackEventArgs(took));
            }
            if (_isFailed)
            {
                Failed?.Invoke(this, new TrackEventArgs(track));
                return false;
            }
            return true;
        }

        private void StartOutput()
        {
            try
            {
                _output.Play();
            }
            catch (IOException)
            {
                _isPlaying = false;
                int track;
                lock (_gate)
                {
                    _isFailed = true;
                    track = _current?.Track ?? 0;
                }
                Failed?.Invoke(this, new TrackEventArgs(track));
            }
        }

        // Of the current track: the position after the chunk, its length, and whether the chunk has its end. And of the
        // incoming track while the transition runs.
        private readonly record struct Chunk(int Track, double Position, long Length, bool IsEnd, double? IncomingPosition, long IncomingLength);

        private sealed class Transition
        {
            public Transition(TransitionPlan plan, long lengthFrames)
            {
                Plan = plan;
                LengthFrames = Math.Max(1, lengthFrames);
            }

            public TransitionPlan Plan { get; }
            public long LengthFrames { get; }
            // The frame where the incoming track came in, or would have for where the transition is.
            public long StartFrame { get; private set; }
            public bool IsStarted { get; private set; }

            public void StartAt(long frame)
            {
                StartFrame = frame;
                IsStarted = true;
            }

            public void Wait() => IsStarted = false;

            public double Progress(long frame) => Math.Clamp((frame - StartFrame) / (double)LengthFrames, 0, 1);
        }

        // The tempo going from where the transition left it to the track's own, from frame to frame.
        private sealed record Ramp(long StartFrame, long EndFrame, double From)
        {
            public double Tempo(long frame) => frame >= EndFrame ? 1 : From + ((1 - From) * (frame - StartFrame) / (EndFrame - StartFrame));
        }
    }
}
