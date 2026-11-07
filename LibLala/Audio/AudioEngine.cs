using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LibLala.Audio
{
    public sealed class TrackEventArgs : EventArgs
    {
        public TrackEventArgs(int track)
        {
            Track = track;
        }

        // The number Open returned for the track.
        public int Track { get; }
    }

    // Plays one track at a time through an output, which asks it for audio on its own thread. The rest is called from one
    // thread. The events are raised on either, without holding a lock of the engine.
    public sealed class AudioEngine : IAudioRenderer, IDisposable
    {
        private readonly Lock _gate = new();
        private readonly IAudioOutput _output;
        // What Render made and the output didn't play yet, oldest first.
        private readonly Queue<Chunk> _rendered = new();
        private IAudioDecoder? _decoder;
        private int _track;
        // The frame of the track that Render reads next.
        private long _renderPosition;
        // The frame of the track that the output played up to.
        private long _playedPosition;
        private bool _isEndRendered;
        private bool _isEndPlayed;
        private bool _isFailed;
        // Only used on the calling thread.
        private bool _isPlaying;

        public AudioEngine(Func<IAudioRenderer, IAudioOutput> createOutput)
        {
            ArgumentNullException.ThrowIfNull(createOutput);
            _output = createOutput(this);
        }

        // The output played the last frame of the track. Raised once, until Seek or Play starts the track again.
        public event EventHandler<TrackEventArgs>? Ended;
        // The track can't be read any more, or the output can't play. Raised once per track.
        public event EventHandler<TrackEventArgs>? Failed;

        public float Volume
        {
            get => _output.Volume;
            set => _output.Volume = value;
        }

        // In frames, 0 while no track is open.
        public long Length
        {
            get
            {
                lock (_gate)
                {
                    return _decoder?.Length ?? 0;
                }
            }
        }

        // In frames. Of what was played, the output can have more of the track waiting to be played.
        public long Position
        {
            get
            {
                lock (_gate)
                {
                    return _playedPosition;
                }
            }
        }

        // Stops what plays and takes over the decoder. Returns the number of the track, the events carry it.
        public int Open(IAudioDecoder decoder)
        {
            ArgumentNullException.ThrowIfNull(decoder);
            return Replace(decoder);
        }

        // Stops what plays and closes the track. Returns a new number, which no event carries.
        public int Close() => Replace(null);

        public void Play()
        {
            bool isAtEnd;
            lock (_gate)
            {
                if (_decoder is null || _isFailed)
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
                isAtEnd = _decoder is null || _isFailed || _isEndPlayed;
                position = _playedPosition;
            }
            if (!isAtEnd)
            {
                MoveTo(position);
            }
        }

        public void Seek(long frame)
        {
            _output.Reset();
            long length;
            lock (_gate)
            {
                if (_decoder is null || _isFailed)
                {
                    return;
                }
                length = _decoder.Length;
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
            int? failedTrack = null;
            lock (_gate)
            {
                var read = 0;
                var isEnd = false;
                if (_decoder is not null && !_isFailed && !_isEndRendered)
                {
                    try
                    {
                        read = _decoder.Read(buffer);
                        isEnd = read < buffer.Length / AudioFormat.Channels;
                    }
                    catch (IOException)
                    {
                        _isFailed = true;
                        failedTrack = _track;
                    }
                }
                buffer[(read * AudioFormat.Channels)..].Clear();
                _renderPosition += read;
                _isEndRendered |= isEnd;
                _rendered.Enqueue(new Chunk(_renderPosition, isEnd));
            }

            if (failedTrack is { } track)
            {
                Failed?.Invoke(this, new TrackEventArgs(track));
            }
        }

        public void Played()
        {
            int? endedTrack = null;
            lock (_gate)
            {
                if (!_rendered.TryDequeue(out var chunk))
                {
                    return;
                }
                _playedPosition = chunk.End;
                if (chunk.IsEnd)
                {
                    _isEndPlayed = true;
                    endedTrack = _track;
                }
            }

            if (endedTrack is { } track)
            {
                Ended?.Invoke(this, new TrackEventArgs(track));
            }
        }

        public void Dispose()
        {
            _output.Dispose();
            lock (_gate)
            {
                _decoder?.Dispose();
                _decoder = null;
            }
        }

        private int Replace(IAudioDecoder? decoder)
        {
            _isPlaying = false;
            _output.Pause();
            _output.Reset();
            IAudioDecoder? old;
            int track;
            lock (_gate)
            {
                old = _decoder;
                _decoder = decoder;
                track = ++_track;
                _isFailed = false;
                _isEndRendered = false;
                _isEndPlayed = false;
                _rendered.Clear();
                _renderPosition = 0;
                _playedPosition = 0;
            }
            old?.Dispose();
            return track;
        }

        // Only while the output is stopped. False when the track failed.
        private bool MoveTo(long frame)
        {
            int track;
            lock (_gate)
            {
                _rendered.Clear();
                _renderPosition = frame;
                _playedPosition = frame;
                _isEndRendered = false;
                _isEndPlayed = false;
                try
                {
                    _decoder?.Seek(frame);
                    return true;
                }
                catch (IOException)
                {
                    _isFailed = true;
                    track = _track;
                }
            }

            Failed?.Invoke(this, new TrackEventArgs(track));
            return false;
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
                    track = _track;
                }
                Failed?.Invoke(this, new TrackEventArgs(track));
            }
        }

        // End is the frame of the track after the chunk.
        private readonly record struct Chunk(long End, bool IsEnd);
    }
}
