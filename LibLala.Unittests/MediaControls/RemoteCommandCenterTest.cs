using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        // 3 by 2 pixels.
        private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAMAAAACCAIAAAASFvFNAAAAEElEQVR4nGP4z8AAQQxwFgBB0gX7h/C5SAAAAABJRU5ErkJggg==");
        private static readonly Lazy<IntPtr> SeekEventClass = new(CreateSeekEventClass);

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
        public void Seek_IsReceived_WithWhereTo()
        {
            using var controls = new RemoteCommandCenter();
            var received = new List<(MediaCommand, TimeSpan)>();
            controls.CommandReceived += (sender, e) => received.Add((e.Command, e.Position));

            var seekEvent = ObjectiveC.Send(ObjectiveC.Send(SeekEventClass.Value, "alloc"), "init");
            try
            {
                SendToATarget("lalamachineSeek:", seekEvent);
            }
            finally
            {
                ObjectiveC.Send(seekEvent, "release");
            }

            Assert.That(received, Is.EqualTo(new[] { (MediaCommand.Seek, TimeSpan.FromSeconds(42.5)) }));
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

        [Test]
        public void Show_WithACover_GivesItsPictureForTheArtwork()
        {
            using var controls = new RemoteCommandCenter();

            controls.Show(Track(Png));

            Assert.That(ArtworkImageSize(), Is.EqualTo((3.0, 2.0)));
        }

        [Test]
        public void Show_WithTheSameCover_KeepsTheArtwork_AnotherCoverReplacesIt()
        {
            using var controls = new RemoteCommandCenter();
            controls.Show(Track(Png));
            // Kept, so that another artwork can't be where it was.
            var artwork = ObjectiveC.Send(NowPlayingValue("MPMediaItemPropertyArtwork"), "retain");
            try
            {
                controls.Show(Track(Png) with { IsPlaying = false });
                Assert.That(NowPlayingValue("MPMediaItemPropertyArtwork"), Is.EqualTo(artwork));

                controls.Show(Track([.. Png]));
                Assert.That(NowPlayingValue("MPMediaItemPropertyArtwork"), Is.Not.EqualTo(artwork));
                Assert.That(ArtworkImageSize(), Is.EqualTo((3.0, 2.0)));
            }
            finally
            {
                ObjectiveC.Send(artwork, "release");
            }
        }

        [Test]
        public void Show_WithACoverThatIsntAPicture_HasNoArtwork()
        {
            using var controls = new RemoteCommandCenter();

            controls.Show(Track([0xFF, 0xD8, 0xFF, 0xE0, 1]));

            Assert.That(NowPlayingString("MPMediaItemPropertyTitle"), Is.EqualTo("a"));
            Assert.That(NowPlayingValue("MPMediaItemPropertyArtwork"), Is.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void Show_WithoutACover_AfterATrackWithOne_HasNoArtwork()
        {
            using var controls = new RemoteCommandCenter();
            controls.Show(Track(Png));

            controls.Show(Track(null));

            Assert.That(NowPlayingValue("MPMediaItemPropertyArtwork"), Is.EqualTo(IntPtr.Zero));
        }

        private static NowPlaying Track(byte[]? cover) => new("a", "", "", TimeSpan.FromSeconds(300), TimeSpan.Zero, IsPlaying: true, cover);

        // Of the picture that the artwork in Now Playing gives, asked like macOS asks it.
        private static (double Width, double Height) ArtworkImageSize()
        {
            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                var image = ObjectiveC.Send(NowPlayingValue("MPMediaItemPropertyArtwork"), ObjectiveC.Selector("imageWithSize:"), new CGSize { Width = 100, Height = 100 });
                Assert.That(image, Is.Not.EqualTo(IntPtr.Zero));
                var size = ObjectiveC.SendForSize(image, ObjectiveC.Selector("size"));
                return (size.Width, size.Height);
            }
            finally
            {
                ObjectiveC.AutoreleasePoolPop(pool);
            }
        }

        // Like macOS: a command calls the method of its target that was given with it. Any target of lalamachine's class.
        private static long SendToATarget(string action, IntPtr commandEvent = default)
        {
            var target = ObjectiveC.Send(ObjectiveC.Send(ObjectiveC.GetClass("LalamachineRemoteCommandTarget"), "alloc"), "init");
            try
            {
                return ObjectiveC.Send(target, action, commandEvent);
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

        // Like MPChangePlaybackPositionCommandEvent, at 42.5 seconds.
        private static unsafe IntPtr CreateSeekEventClass()
        {
            var cls = ObjectiveC.AllocateClassPair(ObjectiveC.GetClass("NSObject"), "LalamachineTestSeekEvent", 0);
            ObjectiveC.AddMethod(cls, ObjectiveC.Selector("positionTime"), (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, double>)&PositionTime, "d@:");
            ObjectiveC.RegisterClassPair(cls);
            return cls;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static double PositionTime(IntPtr self, IntPtr selector) => 42.5;
    }
}
