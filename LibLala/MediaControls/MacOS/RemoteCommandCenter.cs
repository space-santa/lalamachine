using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace LibLala.MediaControls.MacOS
{
    // macOS's controls, from MediaPlayer: MPRemoteCommandCenter has the buttons of headphones, the media keys and the Control
    // Center, MPNowPlayingInfoCenter shows the track there. macOS sends the commands to the app that played last and said what
    // it plays, on the main thread.
    [SupportedOSPlatform("macos")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2216:Disposable types should declare finalizer", Justification = "A finalizer would release the Objective-C objects on its own thread, while macOS can use them. Undisposed, they go with the app.")]
    public sealed class RemoteCommandCenter : IMediaControls
    {
        private const string MediaPlayerPath = "/System/Library/Frameworks/MediaPlayer.framework/MediaPlayer";
        private const string AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";
        // Lalamachine's own Objective-C class for the commands' target.
        private const string TargetClassName = "LalamachineRemoteCommandTarget";

        // MPNowPlayingPlaybackState.
        private const nuint PlayingState = 1;
        private const nuint PausedState = 2;
        private const nuint StoppedState = 3;
        // MPNowPlayingInfoMediaTypeAudio.
        private const nuint AudioMediaType = 1;
        // MPRemoteCommandHandlerStatus.
        private const nint Handled = 0;
        private const nint Failed = 200;

        // Each of MPRemoteCommandCenter's commands that lalamachine handles, and the target's method for it.
        private static readonly (string Getter, string Action, MediaCommand Command)[] Commands =
        [
            ("playCommand", "lalamachinePlay:", MediaCommand.Play),
            ("pauseCommand", "lalamachinePause:", MediaCommand.Pause),
            ("togglePlayPauseCommand", "lalamachineTogglePlayPause:", MediaCommand.TogglePlayPause),
            ("nextTrackCommand", "lalamachineNext:", MediaCommand.Next),
            ("previousTrackCommand", "lalamachinePrevious:", MediaCommand.Previous),
            // The position slider in the Control Center.
            ("changePlaybackPositionCommand", "lalamachineSeek:", MediaCommand.Seek),
        ];

        // Loaded before their classes are looked up, AppKit for NSImage: the app has it already, the tests don't. They stay
        // loaded, a framework can't be unloaded anyway.
        private static readonly IntPtr MediaPlayer = LoadFrameworks();
        // Here, after the frameworks: the runtime sets the static fields in this order, but not before one of them is used.
        // A class looked up before MediaPlayer is loaded is nil, and every message to nil does nothing, without an error.
        private static readonly IntPtr CommandCenterClass = FindClass("MPRemoteCommandCenter");
        private static readonly IntPtr InfoCenterClass = FindClass("MPNowPlayingInfoCenter");
        private static readonly IntPtr ArtworkClass = FindClass("MPMediaItemArtwork");
        private static readonly IntPtr ImageClass = FindClass("NSImage");
        private static readonly Dictionary<IntPtr, MediaCommand> CommandOfAction = Commands.ToDictionary(command => ObjectiveC.Selector(command.Action), command => command.Command);
        private static readonly IntPtr TargetClass = CreateTargetClass();

        // MPMediaItemArtwork asks this block for the picture. All of them get the one picture that's shown, there's only
        // ever one artwork that's shown.
        private static readonly IntPtr ArtworkBlock = CreateArtworkBlock();
        private static readonly Lock ArtworkGate = new();
        // An NSImage, owned. Zero while there's none.
        private static IntPtr s_artworkImage;

        // There's one command center for the app. A command calls the target of each controls that are open, each target's
        // controls get it.
        private static readonly Lock OpenGate = new();
        private static readonly Dictionary<IntPtr, RemoteCommandCenter> OpenByTarget = [];

        private readonly IntPtr _target;
        private readonly IntPtr _infoCenter;
        private readonly IntPtr[] _commands;
        // The cover that the artwork is made of, and the MPMediaItemArtwork, owned. Zero for a cover that isn't a picture.
        private byte[]? _cover;
        private IntPtr _artwork;
        private bool _isDisposed;

        public RemoteCommandCenter()
        {
            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                var commandCenter = ObjectiveC.Send(CommandCenterClass, "sharedCommandCenter");
                _infoCenter = ObjectiveC.Send(InfoCenterClass, "defaultCenter");
                // Owned until Dispose: a command doesn't keep its target.
                _target = ObjectiveC.Send(ObjectiveC.Send(TargetClass, "alloc"), "init");
                _commands = [.. Commands.Select(command => ObjectiveC.Send(commandCenter, command.Getter))];
                lock (OpenGate)
                {
                    OpenByTarget[_target] = this;
                }
                for (var i = 0; i < Commands.Length; i++)
                {
                    ObjectiveC.Send(_commands[i], ObjectiveC.Selector("addTarget:action:"), _target, ObjectiveC.Selector(Commands[i].Action));
                    ObjectiveC.Send(_commands[i], ObjectiveC.Selector("setEnabled:"), true);
                }
            }
            finally
            {
                ObjectiveC.AutoreleasePoolPop(pool);
            }
        }

        public event EventHandler<MediaCommandEventArgs>? CommandReceived;

        // The commands' target, for the tests to call its methods like macOS does. Released when the controls are disposed.
        internal IntPtr Target => _target;

        public void Show(NowPlaying? nowPlaying)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                if (nowPlaying is null)
                {
                    Clear();
                    return;
                }

                var info = ObjectiveC.Send(ObjectiveC.GetClass("NSMutableDictionary"), "dictionary");
                Put(info, "MPMediaItemPropertyTitle", ObjectiveC.String(nowPlaying.Title));
                if (nowPlaying.Artist.Length > 0)
                {
                    Put(info, "MPMediaItemPropertyArtist", ObjectiveC.String(nowPlaying.Artist));
                }
                if (nowPlaying.Album.Length > 0)
                {
                    Put(info, "MPMediaItemPropertyAlbumTitle", ObjectiveC.String(nowPlaying.Album));
                }
                // Made once for each track, not at each play, pause and seek.
                if (!ReferenceEquals(nowPlaying.Cover, _cover))
                {
                    ShowCover(nowPlaying.Cover);
                }
                if (_artwork != IntPtr.Zero)
                {
                    Put(info, "MPMediaItemPropertyArtwork", _artwork);
                }
                Put(info, "MPMediaItemPropertyPlaybackDuration", ObjectiveC.Number(nowPlaying.Duration.TotalSeconds));
                Put(info, "MPNowPlayingInfoPropertyElapsedPlaybackTime", ObjectiveC.Number(nowPlaying.Position.TotalSeconds));
                // At which it counts on from the elapsed time.
                Put(info, "MPNowPlayingInfoPropertyPlaybackRate", ObjectiveC.Number(nowPlaying.IsPlaying ? 1.0 : 0.0));
                Put(info, "MPNowPlayingInfoPropertyDefaultPlaybackRate", ObjectiveC.Number(1.0));
                Put(info, "MPNowPlayingInfoPropertyMediaType", ObjectiveC.Number(AudioMediaType));
                ObjectiveC.Send(_infoCenter, "setNowPlayingInfo:", info);
                // On macOS, unlike on an iPhone, the app says itself whether it plays. It doesn't get the commands without.
                ObjectiveC.Send(_infoCenter, ObjectiveC.Selector("setPlaybackState:"), nowPlaying.IsPlaying ? PlayingState : PausedState);
            }
            finally
            {
                ObjectiveC.AutoreleasePoolPop(pool);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            lock (OpenGate)
            {
                OpenByTarget.Remove(_target);
            }

            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                foreach (var command in _commands)
                {
                    ObjectiveC.Send(command, "removeTarget:", _target);
                    ObjectiveC.Send(command, ObjectiveC.Selector("setEnabled:"), false);
                }
                Clear();
                ObjectiveC.Send(_target, "release");
            }
            finally
            {
                ObjectiveC.AutoreleasePoolPop(pool);
            }
        }

        // The target's methods, all of them, for the selector it was called with: (self, _cmd, MPRemoteCommandEvent *).
        // An exception mustn't get out into the Objective-C runtime, it would end the app.
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Nothing may be thrown back into native code, macOS is told that the command failed instead.")]
        private static nint OnCommand(IntPtr self, IntPtr action, IntPtr commandEvent)
        {
            try
            {
                RemoteCommandCenter? controls;
                lock (OpenGate)
                {
                    OpenByTarget.TryGetValue(self, out controls);
                }
                if (controls is null || !CommandOfAction.TryGetValue(action, out var command))
                {
                    return Failed;
                }
                // MPChangePlaybackPositionCommandEvent's, in seconds.
                var position = command == MediaCommand.Seek ? TimeSpan.FromSeconds(ObjectiveC.SendForDouble(commandEvent, ObjectiveC.Selector("positionTime"))) : TimeSpan.Zero;
                controls.CommandReceived?.Invoke(controls, new MediaCommandEventArgs(command, position));
                return Handled;
            }
            catch (Exception)
            {
                return Failed;
            }
        }

        private static unsafe IntPtr CreateTargetClass()
        {
            var cls = ObjectiveC.AllocateClassPair(ObjectiveC.GetClass("NSObject"), TargetClassName, 0);
            if (cls == IntPtr.Zero)
            {
                // Can't be: the class is made once, here.
                throw new InvalidOperationException($"The Objective-C class {TargetClassName} exists already.");
            }
            var onCommand = (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, nint>)&OnCommand;
            foreach (var action in CommandOfAction.Keys)
            {
                // Returns an NSInteger, takes an object.
                ObjectiveC.AddMethod(cls, action, onCommand, "q@:@");
            }
            ObjectiveC.RegisterClassPair(cls);
            return cls;
        }

        // The artwork block's function: (the block, the size it's for) → NSImage *, autoreleased like what a method returns.
        // The picture as it is, at any size: macOS scales it.
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Nothing may be thrown back into native code, macOS gets no picture instead.")]
        private static IntPtr OnArtworkRequested(IntPtr block, CGSize size)
        {
            try
            {
                lock (ArtworkGate)
                {
                    // Retained here: the picture can be replaced while it's on its way.
                    return s_artworkImage == IntPtr.Zero ? IntPtr.Zero : ObjectiveC.Send(ObjectiveC.Send(s_artworkImage, "retain"), "autorelease");
                }
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        private static unsafe IntPtr CreateArtworkBlock() =>
            // NSImage *(^)(CGSize): returns an object, takes the block and a struct of two doubles.
            ObjectiveC.CreateGlobalBlock((IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, CGSize, IntPtr>)&OnArtworkRequested, "@24@?0{CGSize=dd}8");

        private static IntPtr LoadFrameworks()
        {
            NativeLibrary.Load(AppKitPath);
            return NativeLibrary.Load(MediaPlayerPath);
        }

        // Of the frameworks loaded above.
        private static IntPtr FindClass(string name)
        {
            var cls = ObjectiveC.GetClass(name);
            return cls != IntPtr.Zero ? cls : throw new InvalidOperationException($"macOS has no Objective-C class {name}.");
        }

        // Owned, zero when it isn't a picture that macOS can read.
        private static unsafe IntPtr CreateImage(byte[] picture)
        {
            IntPtr data;
            fixed (byte* bytes = picture)
            {
                // A copy, autoreleased.
                data = ObjectiveC.Send(ObjectiveC.GetClass("NSData"), ObjectiveC.Selector("dataWithBytes:length:"), bytes, (nuint)picture.Length);
            }
            var image = ObjectiveC.Send(ObjectiveC.Send(ImageClass, "alloc"), "initWithData:", data);
            // It takes any data, whether it's a picture shows when it's read.
            if (image != IntPtr.Zero && !ObjectiveC.SendForBool(image, ObjectiveC.Selector("isValid")))
            {
                ObjectiveC.Send(image, "release");
                return IntPtr.Zero;
            }
            return image;
        }

        // Takes the image, gives the one before up.
        private static void SetArtworkImage(IntPtr image)
        {
            IntPtr before;
            lock (ArtworkGate)
            {
                before = s_artworkImage;
                s_artworkImage = image;
            }
            if (before != IntPtr.Zero)
            {
                ObjectiveC.Send(before, "release");
            }
        }

        // Inside a pool.
        private void ShowCover(byte[]? cover)
        {
            _cover = cover;
            if (_artwork != IntPtr.Zero)
            {
                ObjectiveC.Send(_artwork, "release");
                _artwork = IntPtr.Zero;
            }
            var image = cover is null ? IntPtr.Zero : CreateImage(cover);
            SetArtworkImage(image);
            if (image != IntPtr.Zero)
            {
                var size = ObjectiveC.SendForSize(image, ObjectiveC.Selector("size"));
                _artwork = ObjectiveC.Send(ObjectiveC.Send(ArtworkClass, "alloc"), ObjectiveC.Selector("initWithBoundsSize:requestHandler:"), size, ArtworkBlock);
            }
        }

        // Inside a pool.
        private void Clear()
        {
            ObjectiveC.Send(_infoCenter, "setNowPlayingInfo:", IntPtr.Zero);
            ObjectiveC.Send(_infoCenter, ObjectiveC.Selector("setPlaybackState:"), StoppedState);
            ShowCover(null);
        }

        // MediaPlayer's keys are NSString constants that it exports.
        private static void Put(IntPtr dictionary, string key, IntPtr value) => ObjectiveC.Send(dictionary, ObjectiveC.Selector("setObject:forKey:"), value, Marshal.ReadIntPtr(NativeLibrary.GetExport(MediaPlayer, key)));
    }
}
