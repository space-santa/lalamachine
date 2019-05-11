using System.Windows;
using System.Windows.Controls;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlayerButtonsControl.xaml
    /// </summary>
    public partial class PlayerButtonsControl : UserControl
    {
        public PlayerButtonsControl()
        {
            InitializeComponent();
        }

        public static readonly RoutedEvent BackEvent = EventManager.RegisterRoutedEvent(
            "Back", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlayerButtonsControl));
        public event RoutedEventHandler Back
        {
            add { AddHandler(BackEvent, value); }
            remove { RemoveHandler(BackEvent, value); }
        }
        void RaiseBackEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PlayerButtonsControl.BackEvent);
            RaiseEvent(newEventArgs);
        }

        public static readonly RoutedEvent ForwardEvent = EventManager.RegisterRoutedEvent(
            "Forward", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlayerButtonsControl));
        public event RoutedEventHandler Forward
        {
            add { AddHandler(ForwardEvent, value); }
            remove { RemoveHandler(ForwardEvent, value); }
        }
        void RaiseForwardEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PlayerButtonsControl.ForwardEvent);
            RaiseEvent(newEventArgs);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseBackEvent();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseForwardEvent();
        }
    }
}
