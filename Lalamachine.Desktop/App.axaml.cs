using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Lalamachine.Desktop.Views;

namespace Lalamachine.Desktop
{
    public partial class App : Application
    {
        private static string ErrorLogPath => Path.Combine(LibLala.Constants.LALADIR, "error.log");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            LibLala.Utils.DirectoryEnforcer.EnsureLaladir();

            Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogException(e.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogException(e.Exception);
                e.SetObserved();
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = CreateMainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        // E.g. VLC isn't installed or the database is broken. Without a window, the app would just not open.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Whatever went wrong, the user has to be told.")]
        private static Window CreateMainWindow()
        {
            Window? window = null;
            try
            {
                window = new MainWindow();
                // Here, rather than leaving it to the lifetime: what goes wrong in the first layout doesn't reach the dispatcher's handler.
                window.Show();
                return window;
            }
            catch (Exception e)
            {
                LogException(e);
                var message = new MessageWindow($"Lalamachine couldn't start: {e.Message}\n\nThe details are in {ErrorLogPath}")
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                };
                // In this order, closing the last window ends the app.
                message.Show();
                window?.Close();
                return message;
            }
        }

        private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);

            // Once the window is up, keep going (and keep the music playing). If starting up failed, there is nothing to keep.
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { IsLoaded: true } window })
            {
                e.Handled = true;
                _ = MessageWindow.ShowAsync(window, $"Something went wrong: {e.Exception.Message}\n\nThe details are in {ErrorLogPath}");
            }
        }

        private static void LogException(object exception)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("O", CultureInfo.InvariantCulture);
                File.AppendAllText(ErrorLogPath, $"{timestamp}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
            }
            catch (IOException)
            {
                // Nowhere left to report it.
            }
            catch (UnauthorizedAccessException)
            {
                // Nowhere left to report it.
            }
        }
    }
}
