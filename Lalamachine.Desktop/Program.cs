using System;
using Avalonia;

namespace Lalamachine.Desktop
{
    internal static class Program
    {
        // Avalonia isn't initialized before AppMain is called, so don't use it (or anything that needs a SynchronizationContext) before that.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Also used by the visual designer.
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }
}
