using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Lalamachine.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string ErrorLogPath => Path.Combine(LibLala.Constants.LALADIR, "error.log");

        public App()
        {
            LibLala.Utils.DirectoryEnforcer.EnsureLaladir();

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogException(e.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogException(e.Exception);
                e.SetObserved();
            };
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            MessageBox.Show($"Something went wrong: {e.Exception.Message}\n\nThe details are in {ErrorLogPath}");

            // Once the window is up, keep going (and keep the music playing). If starting up failed, there is nothing to keep.
            e.Handled = true;
            if (MainWindow?.IsLoaded != true)
            {
                Shutdown(1);
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
