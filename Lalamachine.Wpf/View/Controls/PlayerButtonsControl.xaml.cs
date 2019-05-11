using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public static readonly RoutedEvent LoadEvent = EventManager.RegisterRoutedEvent(
            "Load", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlayerButtonsControl));
        public event RoutedEventHandler Load
        {
            add { AddHandler(LoadEvent, value); }
            remove { RemoveHandler(LoadEvent, value); }
        }
        void RaiseLoadEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PlayerButtonsControl.LoadEvent);
            RaiseEvent(newEventArgs);
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

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseLoadEvent();
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
