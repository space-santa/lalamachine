using System;

namespace LibLala.Audio.Mixing
{
    // Keeps the peaks of a mix under a ceiling without clipping them. The audio comes out of it LookAheadFrames late, so it
    // sees a peak coming: it turns the gain down smoothly over the frames before it, and back up slowly after. Both
    // channels by the same gain.
    internal sealed class Limiter
    {
        public const int LookAheadFrames = 96;
        public const float Ceiling = 0.95f;
        // Faster, it would turn bass up and down within its waves. Slower, it would hold the gain down after a kick longer.
        private const double ReleaseSeconds = 0.1;
        // Closer to 1 than this, the gain is back at 1.
        private const double Snap = 1e-4;
        private static readonly double ReleaseStep = 1 - Math.Exp(-1 / (ReleaseSeconds * AudioFormat.SampleRate));

        private readonly float[] _delay = new float[LookAheadFrames * AudioFormat.Channels];
        // Of the frame going out and the LookAheadFrames after it, the gains they need that can still be the smallest:
        // rising, oldest first.
        private readonly double[] _needed = new double[LookAheadFrames + 1];
        private readonly long[] _neededFrame = new long[LookAheadFrames + 1];
        // The last LookAheadFrames of the gain envelope. Their average is the gain, so it goes down over them.
        private readonly double[] _envelope = new double[LookAheadFrames];
        private int _neededFirst;
        private int _neededCount;
        private double _envelopeSum;
        private int _envelopeBelowOne;
        private double _gain;
        private long _frame;
        // In _delay and _envelope.
        private int _position;

        public Limiter()
        {
            Clear();
        }

        // Frames taken in that didn't come out yet: LookAheadFrames from Prime until Drain.
        public int Held { get; private set; }

        // Nothing it holds is turned down, nor will be: what it holds can come out as it is.
        public bool IsIdle => _envelopeBelowOne == 0 && _gain >= 1 && (_neededCount == 0 || _needed[_neededFirst] >= 1);

        public void Clear()
        {
            Array.Clear(_delay);
            Array.Fill(_envelope, 1);
            _envelopeSum = LookAheadFrames;
            _envelopeBelowOne = 0;
            _neededFirst = 0;
            _neededCount = 0;
            _gain = 1;
            _position = 0;
            Held = 0;
        }

        // Takes the first frames in, without putting any out, until it holds LookAheadFrames.
        public void Prime(ReadOnlySpan<float> frames)
        {
            for (var i = 0; i < frames.Length; i += AudioFormat.Channels)
            {
                Take(frames[i], frames[i + 1], limit: true);
                Held = Math.Min(Held + 1, LookAheadFrames);
            }
        }

        // Takes frames in and puts as many out, of LookAheadFrames before. Only while limiting it turns peaks down, else the
        // gain goes back up.
        public void Process(ReadOnlySpan<float> input, Span<float> output, bool limit)
        {
            for (var i = 0; i < input.Length; i += AudioFormat.Channels)
            {
                (output[i], output[i + 1]) = Take(input[i], input[i + 1], limit);
            }
        }

        // Puts out what it holds, oldest first, as far as the output goes. Only when it's idle, the frames come out as they
        // are. Returns how many.
        public int Drain(Span<float> output)
        {
            var frames = Math.Min(Held, output.Length / AudioFormat.Channels);
            for (var i = 0; i < frames; i++)
            {
                var at = (_position + LookAheadFrames - Held) % LookAheadFrames;
                output[i * AudioFormat.Channels] = _delay[at * AudioFormat.Channels];
                output[(i * AudioFormat.Channels) + 1] = _delay[(at * AudioFormat.Channels) + 1];
                Held--;
            }
            return frames;
        }

        // The frame in, and the one from LookAheadFrames before out, by the gain it needs.
        private (float Left, float Right) Take(float left, float right, bool limit)
        {
            var peak = Math.Max(Math.Abs(left), Math.Abs(right));
            var needed = limit && peak > Ceiling ? Ceiling / peak : 1;

            // The smallest gain that the frame going out and the ones after it need. Before it, the gain goes down to it.
            if (_neededCount > 0 && _neededFrame[_neededFirst] < _frame - LookAheadFrames)
            {
                _neededFirst = (_neededFirst + 1) % _needed.Length;
                _neededCount--;
            }
            while (_neededCount > 0 && _needed[(_neededFirst + _neededCount - 1) % _needed.Length] >= needed)
            {
                _neededCount--;
            }
            var last = (_neededFirst + _neededCount) % _needed.Length;
            _needed[last] = needed;
            _neededFrame[last] = _frame;
            _neededCount++;
            var smallest = _needed[_neededFirst];

            _gain = smallest < _gain ? smallest : _gain + ((smallest - _gain) * ReleaseStep);
            if (smallest >= 1 && _gain > 1 - Snap)
            {
                _gain = 1;
            }

            // Averaged over the frames ahead, it's down to what a peak needs when the peak comes out.
            var old = _envelope[_position];
            _envelope[_position] = _gain;
            _envelopeBelowOne += (_gain < 1 ? 1 : 0) - (old < 1 ? 1 : 0);
            _envelopeSum = _envelopeBelowOne == 0 ? LookAheadFrames : _envelopeSum + _gain - old;
            var gain = (float)(_envelopeSum / LookAheadFrames);

            var at = _position * AudioFormat.Channels;
            var out0 = _delay[at] * gain;
            var out1 = _delay[at + 1] * gain;
            _delay[at] = left;
            _delay[at + 1] = right;
            _position = (_position + 1) % LookAheadFrames;
            _frame++;
            return (out0, out1);
        }
    }
}
