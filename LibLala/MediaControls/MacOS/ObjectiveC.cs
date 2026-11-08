using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LibLala.MediaControls.MacOS
{
    // The Objective-C runtime, for macOS's frameworks that have no C API, like MediaPlayer. A message is sent with
    // objc_msgSend, declared once for each of the signatures used here: it jumps to the method with the arguments as they
    // are, so it has to be called like the method it ends up in.
    [SupportedOSPlatform("macos")]
    internal static unsafe partial class ObjectiveC
    {
        private const string LibObjC = "/usr/lib/libobjc.A.dylib";

        [LibraryImport(LibObjC, EntryPoint = "objc_getClass", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr GetClass(string name);

        // The same pointer for the same name, it can be compared.
        [LibraryImport(LibObjC, EntryPoint = "sel_registerName", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr Selector(string name);

        // Zero when a class with that name exists.
        [LibraryImport(LibObjC, EntryPoint = "objc_allocateClassPair", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr AllocateClassPair(IntPtr superclass, string name, nint extraBytes);

        [LibraryImport(LibObjC, EntryPoint = "objc_registerClassPair")]
        public static partial void RegisterClassPair(IntPtr cls);

        // Types like "q@:@": what it returns, then self, the selector and the arguments.
        [LibraryImport(LibObjC, EntryPoint = "class_addMethod", StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static partial bool AddMethod(IntPtr cls, IntPtr selector, IntPtr implementation, string types);

        // What's autoreleased in between is released by the pop.
        [LibraryImport(LibObjC, EntryPoint = "objc_autoreleasePoolPush")]
        public static partial IntPtr AutoreleasePoolPush();

        [LibraryImport(LibObjC, EntryPoint = "objc_autoreleasePoolPop")]
        public static partial void AutoreleasePoolPop(IntPtr pool);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr argument);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr first, IntPtr second);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector, double argument);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector, nuint argument);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.U1)] bool argument);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial IntPtr Send(IntPtr receiver, IntPtr selector, char* characters, nuint length);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial nuint SendForUnsigned(IntPtr receiver, IntPtr selector);

        [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
        public static partial double SendForDouble(IntPtr receiver, IntPtr selector);

        public static IntPtr Send(IntPtr receiver, string selector) => Send(receiver, Selector(selector));

        public static IntPtr Send(IntPtr receiver, string selector, IntPtr argument) => Send(receiver, Selector(selector), argument);

        // Autoreleased, so only inside a pool.
        public static IntPtr String(string value)
        {
            fixed (char* characters = value)
            {
                return Send(GetClass("NSString"), Selector("stringWithCharacters:length:"), characters, (nuint)value.Length);
            }
        }

        // Autoreleased, so only inside a pool.
        public static IntPtr Number(double value) => Send(GetClass("NSNumber"), Selector("numberWithDouble:"), value);

        // Autoreleased, so only inside a pool.
        public static IntPtr Number(nuint value) => Send(GetClass("NSNumber"), Selector("numberWithUnsignedInteger:"), value);

        public static string? ToManagedString(IntPtr nsString) => nsString == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(Send(nsString, "UTF8String"));
    }
}
