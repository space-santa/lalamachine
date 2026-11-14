using System;
using System.Collections.Generic;
using System.Linq;
using LibLala.Audio;
using LibLala.Audio.Mixing;
using NUnit.Framework;

namespace LibLala.Unittests.Audio.Mixing
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The deck takes over the decoder.")]
    public class DeckTest
    {
        [Test]
        public void AtItsOwnTempo_ItIsReadAsItIs()
        {
            using var deck = new Deck(new CountingDecoder(10000), null, 1);
            var buffer = new float[1000 * AudioFormat.Channels];

            deck.Render(buffer);

            Assert.That(deck.Position, Is.EqualTo(1000));
            Assert.That(buffer[999 * AudioFormat.Channels], Is.EqualTo(999));
        }

        [TestCase(0.95)]
        [TestCase(1.04)]
        public void Stretched_ItsPositionGoesByTheTempo(double tempo)
        {
            using var deck = new Deck(new DrumDecoder(128, 0.1, seconds: 30), null, 1) { Tempo = tempo };
            var buffer = new float[AudioFormat.SampleRate * 10 * AudioFormat.Channels];

            var rendered = deck.Render(buffer);

            Assert.That(rendered, Is.EqualTo(AudioFormat.SampleRate * 10));
            // Within the 5 ms that SoundTouch's overlaps move things.
            Assert.That(deck.Position, Is.EqualTo(AudioFormat.SampleRate * 10 * tempo).Within(0.005 * AudioFormat.SampleRate));
        }

        [Test]
        public void Stretched_ItEnds_AfterAllOfTheTrack()
        {
            using var deck = new Deck(new DrumDecoder(128, 0.1, seconds: 5), null, 1) { Tempo = 0.9 };
            var buffer = new float[AudioFormat.SampleRate * AudioFormat.Channels];
            var rendered = 0;

            for (var i = 0; i < 10 && !deck.IsEnded; i++)
            {
                rendered += deck.Render(buffer);
            }

            Assert.That(deck.IsEnded, Is.True);
            Assert.That(rendered, Is.EqualTo(5 / 0.9 * AudioFormat.SampleRate).Within(0.2 * AudioFormat.SampleRate));
        }

        // Like a track that starts going to the next one's tempo while it plays.
        [Test]
        public void GoingIntoTheStretching_ItGoesOnWithoutAJump()
        {
            using var deck = new Deck(new SineDecoder(440, 100000), null, 1);
            var step = new float[256 * AudioFormat.Channels];
            var heard = new List<float>();
            for (var i = 0; i < 40; i++)
            {
                if (i == 20)
                {
                    deck.Tempo = 1.0001;
                }
                var before = deck.Position;
                deck.Render(step);
                Assert.That(deck.Position - before, Is.EqualTo(256 * deck.Tempo).Within(1), $"step {i}");
                // Nor quieter, the sine peaks in every step.
                Assert.That(Enumerable.Range(0, 256).Max(f => Math.Abs(step[f * AudioFormat.Channels])), Is.GreaterThan(0.49), $"step {i}");
                heard.AddRange(Enumerable.Range(0, 256).Select(f => step[f * AudioFormat.Channels]));
            }

            // A 440 Hz sine at 0.5 moves by at most this from one frame to the next.
            var most = 0.5 * 2 * Math.PI * 440 / AudioFormat.SampleRate;
            var jumps = Enumerable.Range(1, heard.Count - 1).Select(f => Math.Abs(heard[f] - heard[f - 1])).ToList();
            Assert.That(jumps.Max(), Is.LessThan(most * 1.1), $"at frame {jumps.IndexOf(jumps.Max()) + 1}");
        }

        [Test]
        public void Seek_AtItsOwnTempo_ReadsAsItIsAgain()
        {
            using var deck = new Deck(new CountingDecoder(100000), null, 1) { Tempo = 0.95 };
            var buffer = new float[1000 * AudioFormat.Channels];
            deck.Render(buffer);

            deck.Tempo = 1;
            deck.Seek(5000);
            deck.Render(buffer);

            Assert.That(buffer[0], Is.EqualTo(5000));
            Assert.That(deck.Position, Is.EqualTo(6000));
        }
    }
}
