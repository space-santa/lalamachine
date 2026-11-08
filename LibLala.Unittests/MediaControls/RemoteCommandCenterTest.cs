using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using LibLala.MediaControls;
using LibLala.MediaControls.MacOS;
using NUnit.Framework;

namespace LibLala.Unittests.MediaControls
{
    [SupportedOSPlatform("macos")]
    public class RemoteCommandCenterTest
    {
        [SetUp]
        public void SetUp()
        {
            if (!OperatingSystem.IsMacOS())
            {
                Assert.Ignore("Uses macOS's MediaPlayer.");
            }
        }

        [TestCase("lalamachinePlay:", MediaCommand.Play)]
        [TestCase("lalamachinePause:", MediaCommand.Pause)]
        [TestCase("lalamachineTogglePlayPause:", MediaCommand.TogglePlayPause)]
        [TestCase("lalamachineNext:", MediaCommand.Next)]
        [TestCase("lalamachinePrevious:", MediaCommand.Previous)]
        public void ACommand_IsReceived(string action, MediaCommand expected)
        {
            using var controls = new RemoteCommandCenter();
            var received = new List<MediaCommand>();
            controls.CommandReceived += (sender, e) => received.Add(e.Command);

            var status = SendToATarget(action);

            Assert.That(received, Is.EqualTo(new[] { expected }));
            Assert.That(status, Is.Zero, "MPRemoteCommandHandlerStatusSuccess");
        }

        [Test]
        public void ACommand_AfterDispose_IsNotReceived_AndFails()
        {
            var received = new List<MediaCommand>();
            var controls = new RemoteCommandCenter();
            controls.CommandReceived += (sender, e) => received.Add(e.Command);
            controls.Dispose();

            var status = SendToATarget("lalamachinePlay:");

            Assert.That(received, Is.Empty);
            Assert.That(status, Is.Not.Zero);
        }

        [Test]
        public void Show_PutsTheTrackInNowPlaying()
        {
            using var controls = new RemoteCommandCenter();

            controls.Show(new NowPlaying("Netherworld", "Somebody, Someone Else", "Underground", TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(42), IsPlaying: false));

            Assert.That(NowPlayingString("MPMediaItemPropertyTitle"), Is.EqualTo("Netherworld"));
            Assert.That(NowPlayingString("MPMediaItemPropertyArtist"), Is.EqualTo("Somebody, Someone Else"));
            Assert.That(NowPlayingString("MPMediaItemPropertyAlbumTitle"), Is.EqualTo("Underground"));
            Assert.That(NowPlayingDouble("MPMediaItemPropertyPlaybackDuration"), Is.EqualTo(300));
            Assert.That(NowPlayingDouble("MPNowPlayingInfoPropertyElapsedPlaybackTime"), Is.EqualTo(42));
            Assert.That(NowPlayingDouble("MPNowPlayingInfoPropertyPlaybackRate"), Is.Zero);
            Assert.That(PlaybackState(), Is.EqualTo(2), "MPNowPlayingPlaybackStatePaused");
        }

        [Test]
        public void Show_WhilePlaying_CountsOn()
        {
            using var controls = new RemoteCommandCenter();

            controls.Show(new NowPlaying("Ünïcödé ♫", "", "", TimeSpan.FromSeconds(300), TimeSpan.Zero, IsPlaying: true));

            Assert.That(NowPlayingString("MPMediaItemPropertyTitle"), Is.EqualTo("Ünïcödé ♫"));
            Assert.That(NowPlayingString("MPMediaItemPropertyArtist"), Is.Null);
            Assert.That(NowPlayingDouble("MPNowPlayingInfoPropertyPlaybackRate"), Is.EqualTo(1));
            Assert.That(PlaybackState(), Is.EqualTo(1), "MPNowPlayingPlaybackStatePlaying");
        }

        [Test]
        public void ShowNothing_AndDispose_TakeTheTrackOutOfNowPlaying()
        {
            using (var controls = new RemoteCommandCenter())
            {
                controls.Show(new NowPlaying("a", "", "", TimeSpan.FromSeconds(1), TimeSpan.Zero, IsPlaying: true));
                controls.Show(null);

                Assert.That(NowPlayingInfo(), Is.EqualTo(IntPtr.Zero));
                Assert.That(PlaybackState(), Is.EqualTo(3), "MPNowPlayingPlaybackStateStopped");

                controls.Show(new NowPlaying("a", "", "", TimeSpan.FromSeconds(1), TimeSpan.Zero, IsPlaying: true));
            }

            Assert.That(NowPlayingInfo(), Is.EqualTo(IntPtr.Zero));
            Assert.That(PlaybackState(), Is.EqualTo(3), "MPNowPlayingPlaybackStateStopped");
        }

        // Like macOS: a command calls the method of its target that was given with it. Any target of lalamachine's class.
        private static long SendToATarget(string action)
        {
            var target = ObjectiveC.Send(ObjectiveC.Send(ObjectiveC.GetClass("LalamachineRemoteCommandTarget"), "alloc"), "init");
            try
            {
                return ObjectiveC.Send(target, action, IntPtr.Zero);
            }
            finally
            {
                ObjectiveC.Send(target, "release");
            }
        }

        private static IntPtr InfoCenter => ObjectiveC.Send(ObjectiveC.GetClass("MPNowPlayingInfoCenter"), "defaultCenter");

        private static IntPtr NowPlayingInfo() => ObjectiveC.Send(InfoCenter, "nowPlayingInfo");

        private static int PlaybackState() => (int)ObjectiveC.SendForUnsigned(InfoCenter, ObjectiveC.Selector("playbackState"));

        private static IntPtr NowPlayingValue(string key)
        {
            var library = NativeLibrary.Load("/System/Library/Frameworks/MediaPlayer.framework/MediaPlayer");
            return ObjectiveC.Send(NowPlayingInfo(), "objectForKey:", Marshal.ReadIntPtr(NativeLibrary.GetExport(library, key)));
        }

        private static string? NowPlayingString(string key)
        {
            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                return ObjectiveC.ToManagedString(NowPlayingValue(key));
            }
            finally
            {
                ObjectiveC.AutoreleasePoolPop(pool);
            }
        }

        private static double NowPlayingDouble(string key)
        {
            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                return ObjectiveC.SendForDouble(NowPlayingValue(key), ObjectiveC.Selector("doubleValue"));
            }
            finally
            {
                ObjectiveC.AutoreleasePoolPop(pool);
            }
        }
    }
}
