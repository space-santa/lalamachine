using System.Windows;

namespace Lalamachine.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            LibLala.Utils.DirectoryEnforcer.EnsureLaladir();
        }
    }
}
