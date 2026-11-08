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
    public sealed class RemoteCommandCenter : IMediaControls
    {
        private const string MediaPlayerPath = "/System/Library/Frameworks/MediaPlayer.framework/MediaPlayer";
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
        ];

        // Loaded before its classes are looked up. It stays loaded, a framework can't be unloaded anyway.
        private static readonly IntPtr MediaPlayer = NativeLibrary.Load(MediaPlayerPath);
        private static readonly Dictionary<IntPtr, MediaCommand> CommandOfAction = Commands.ToDictionary(command => ObjectiveC.Selector(command.Action), command => command.Command);
        private static readonly IntPtr TargetClass = CreateTargetClass();

        // There's one command center for the app, its commands go to the controls that were opened last.
        private static readonly Lock OpenGate = new();
        private static RemoteCommandCenter? s_open;

        private readonly IntPtr _target;
        private readonly IntPtr _infoCenter;
        private readonly IntPtr[] _commands;
        private bool _isDisposed;

        public RemoteCommandCenter()
        {
            var pool = ObjectiveC.AutoreleasePoolPush();
            try
            {
                var commandCenter = ObjectiveC.Send(ObjectiveC.GetClass("MPRemoteCommandCenter"), "sharedCommandCenter");
                _infoCenter = ObjectiveC.Send(ObjectiveC.GetClass("MPNowPlayingInfoCenter"), "defaultCenter");
                // Owned until Dispose: a command doesn't keep its target.
                _target = ObjectiveC.Send(ObjectiveC.Send(TargetClass, "alloc"), "init");
                _commands = [.. Commands.Select(command => ObjectiveC.Send(commandCenter, command.Getter))];
                lock (OpenGate)
                {
                    s_open = this;
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
                if (s_open == this)
                {
                    s_open = null;
                }
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
                RemoteCommandCenter? open;
                lock (OpenGate)
                {
                    open = s_open;
                }
                if (open is null || !CommandOfAction.TryGetValue(action, out var command))
                {
                    return Failed;
                }
                open.CommandReceived?.Invoke(open, new MediaCommandEventArgs(command));
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

        private void Clear()
        {
            ObjectiveC.Send(_infoCenter, "setNowPlayingInfo:", IntPtr.Zero);
            ObjectiveC.Send(_infoCenter, ObjectiveC.Selector("setPlaybackState:"), StoppedState);
        }

        // MediaPlayer's keys are NSString constants that it exports.
        private static void Put(IntPtr dictionary, string key, IntPtr value) => ObjectiveC.Send(dictionary, ObjectiveC.Selector("setObject:forKey:"), value, Marshal.ReadIntPtr(NativeLibrary.GetExport(MediaPlayer, key)));
    }
}
